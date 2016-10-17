﻿using System.Threading.Tasks;
using App.Metrics.Health;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace App.Metrics.Facts.DependencyInjection
{
    public class HealthCheckRegistryTests
    {
        [Fact]
        public async Task can_register_inline_health_checks()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDatabase, Database>();
            services.AddMetrics(
                options =>
                {
                    options.HealthCheckRegistry = checks => { checks.Register("DatabaseConnected", () => Task.FromResult("Database Connection OK")); };
                });
            var provider = services.BuildServiceProvider();
            var metricsContext = provider.GetRequiredService<IMetricsContext>();

            var result = await metricsContext.Advanced.HealthCheckDataProvider.GetStatusAsync();

            result.HasRegisteredChecks.Should().BeTrue();
            result.Results.Should().HaveCount(2);
        }

        [Fact]
        public async Task should_report_healthy_when_all_checks_pass()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDatabase, Database>();
            services.AddMetrics();

            var provider = services.BuildServiceProvider();
            var metricsContext = provider.GetRequiredService<IMetricsContext>();

            var result = await metricsContext.Advanced.HealthCheckDataProvider.GetStatusAsync();

            result.IsHealthy.Should().BeTrue();
        }

        [Fact]
        public async Task should_report_unhealthy_when_all_checks_pass()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDatabase, Database>();
            services.AddMetrics(
                options =>
                {
                    options.HealthCheckRegistry =
                        checks => { checks.Register("DatabaseConnected", () => Task.FromResult(HealthCheckResult.Unhealthy("Failed"))); };
                });
            var provider = services.BuildServiceProvider();
            var metricsContext = provider.GetRequiredService<IMetricsContext>();

            var result = await metricsContext.Advanced.HealthCheckDataProvider.GetStatusAsync();

            result.IsHealthy.Should().BeFalse();
        }

        [Fact]
        public async Task should_scan_assembly_and_register_health_checks_and_ignore_obsolete_checks()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDatabase, Database>();
            services.AddMetrics();

            var provider = services.BuildServiceProvider();
            var metricsContext = provider.GetRequiredService<IMetricsContext>();

            var result = await metricsContext.Advanced.HealthCheckDataProvider.GetStatusAsync();

            result.HasRegisteredChecks.Should().BeTrue();
            result.Results.Should().HaveCount(1);
        }
    }
}