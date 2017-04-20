using System;
using Magicube.Actor.Domain;
using Magicube.Actor.SchedulerClient.Jobs;
using Quartz;

namespace Magicube.Actor.SchedulerClient.Modules {
    public class CastingJobDescription {
        private readonly JobDescription _jobDescription;

        public CastingJobDescription(JobDescription model) {
            _jobDescription = model;
        }

        public IJobDetail RetrieveJobDetail(Action<TransferContext> reportHandler) {
            var jobDetail = JobBuilder
                .Create<JobsHandler>()
                .SetJobData(new JobDataMap { { "Action", reportHandler }, { "Desc", _jobDescription.Description } })
                .WithIdentity(_jobDescription.JobName, _jobDescription.JobGroup);

            if (_jobDescription.JobData != null) {
                jobDetail.UsingJobData("JobData", true);
            }

            return jobDetail.Build();
        }

        public ITrigger RetrieveJobTrigger() {
            var builder = TriggerBuilder.Create()
                .WithIdentity(_jobDescription.JobName, _jobDescription.JobGroup);

            if (_jobDescription.JobData != null) {
                builder.UsingJobData(new JobDataMap(_jobDescription.JobData.ToDictionary()));
            }

            switch (_jobDescription.Schedule.IntervalType) {
                case IntervalUnit.Second:
                case IntervalUnit.Minute:
                case IntervalUnit.Hour: {
                        var sampleJob = (SampleJobSchedule)_jobDescription.Schedule;
                        var timespan = BuildInterval(sampleJob.IntervalType, sampleJob.IntervalStep);
                        builder.WithSimpleSchedule(ssb => ssb.WithInterval(timespan).RepeatForever());
                    }
                    break;
                case IntervalUnit.Day: {
                        var dayJob = (DailyJobSchedule)_jobDescription.Schedule;
                        builder.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(dayJob.TimeOf.Hour, dayJob.TimeOf.Minute)).StartNow();
                    }
                    break;
                case IntervalUnit.Week: {
                        var weekJob = (DailyJobSchedule)_jobDescription.Schedule;
                        builder.WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Saturday, weekJob.TimeOf.Hour, weekJob.TimeOf.Minute)).StartNow();
                    }
                    break;
                case IntervalUnit.Month: {
                        var monthJob = (DailyJobSchedule)_jobDescription.Schedule;
                        builder.WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(30, monthJob.TimeOf.Hour, monthJob.TimeOf.Minute));
                    }
                    break;
                case IntervalUnit.Year: {
                        var yearJob = (DailyJobSchedule)_jobDescription.Schedule;
                        DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            yearJob.TimeOf.Hour, yearJob.TimeOf.Minute, yearJob.TimeOf.Second, DateTimeKind.Local);
                        builder.WithCalendarIntervalSchedule(x => x.WithIntervalInYears(1)).StartAt(new DateTimeOffset(startDate));
                    }
                    break;
            }

            return builder.Build();
        }

        private TimeSpan BuildInterval(IntervalUnit unit, long step) {
            switch (unit) {
                case IntervalUnit.Second:
                    return TimeSpan.FromSeconds(step);
                case IntervalUnit.Hour:
                    return TimeSpan.FromHours(step);
                case IntervalUnit.Minute:
                    return TimeSpan.FromMinutes(step);
                default:
                    return TimeSpan.FromMinutes(step);
            }
        }
    }
}
