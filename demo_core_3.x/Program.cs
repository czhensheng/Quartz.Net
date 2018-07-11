using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace demo_core_3.x
{
    class Program
    {
        static void Main(string[] args)
        {
            Show().GetAwaiter().GetResult();
        }

        public static async Task Show()
        {
            try
            {
                //NameValueCollection props = new NameValueCollection
                //{
                //    { "quartz.serializer.type","binary"}
                //};
                //StdSchedulerFactory factory = new StdSchedulerFactory(props);

                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();
                await scheduler.Start();
                //定义一个job
                IJobDetail job = JobBuilder.Create<HelloJob>().WithIdentity("job1", "group1").Build();
                ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                    .WithIdentity("trigger1")  //给任务起一个名字
                    .StartAt(DateTime.Now) //设置开始时间
                    .ForJob("job1", "group1") //给任务指定一个分组
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(3).RepeatForever())//循环时间
                    .Build();
                //等待执行任务
                await scheduler.ScheduleJob(job, trigger);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message.ToString());
            }

        }

    }



    class HelloJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("hello czs");
            return Task.CompletedTask;
        }
    }
}
