using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace demo_fx_2._6
{
    class Program
    {
        static void Main(string[] args)
        {
            //从工厂中获取一个调度器实例化
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            //开启调度器
            scheduler.Start();
            //JobDetail
            //JobDetail表示一个具体的可执行的调度程序，Job是这个可执行程调度程序所要执行的内容，另外JobDetail还包含了这个任务调度的方案和策略。
            //创建作业
            IJobDetail job1 = JobBuilder.Create<TestJob1>()
                .WithIdentity("myJob1", "group1")
                .Build();
            IJobDetail job2 = JobBuilder.Create<TestJob2>()
                .WithIdentity("myJob2", "group2")
                .UsingJobData("jobSays", "helloworld")
                .UsingJobData("myFloatValue", 3.141f)
                .Build();
            //Trigger代表一个调度参数的配置，什么时候去调

            ITrigger tigger1 = TriggerBuilder.Create()
                .WithIdentity("tiggerName1", "tiggerGroup1")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever())
                .StartAt(DateTime.Now.AddSeconds(5))
                .EndAt(DateTime.Now.AddSeconds(20))
                .Build();
            ITrigger tigger2 = TriggerBuilder.Create().WithIdentity("tiggerName2", "tiggerGroup2").WithSimpleSchedule
                (x => x.WithIntervalInSeconds(1).WithRepeatCount(10)).Build();
            //AOP切入
            scheduler.ListenerManager.AddJobListener(new MyAopListener(), GroupMatcher<JobKey>.AnyGroup());

            //把作业，触发器加入调度器。
            //Scheduler代表一个调度容器，一个调度容器中可以注册多个JobDetail和Trigger。当Trigger与JobDetail组合，就可以被Scheduler容器调度了。
            scheduler.ScheduleJob(job2, tigger2);
            scheduler.ScheduleJob(job1, tigger1);
        }
    }

    /// <summary>
    /// 需要执行的job 需要继承IJob  然后实现exc方法
    /// 表示一个工作，要执行的具体内容
    /// </summary>
    /// 加此特性头  表示执行完毕再触发
    [DisallowConcurrentExecution]
    class TestJob1 : IJob
    {
        private static int ac = 0;
        private static int ab = 0;


        public void Execute(IJobExecutionContext context)
        {
            ab++;
            Console.WriteLine("任务一调度成功" + ab + "次" + DateTime.Now.ToString());
        }
    }

    class TestJob2 : IJob
    {
        private static int ac = 0;

        ///每一次执行任务的时候都是在实例化一个job类（继承自IJob接口）
        /// 可用下面的构造函数来测试 ，每次实例化时将ac置0
        //public TestJob2()
        //{
        //    ac = 0;
        //}
        /// <summary>
        /// 请注意，Execute 方法接受一个 JobExecutionContext对象作为参数。这个对象提供了作业实例的运行时上下文。特别地，它提供了对调度器和触发器的访问，这两者协作来启动作业以及作业的 JobDetail对象的执行。Quartz.NET 通过把作业的状态放在 JobDetail 对象中并让 JobDetail构造函数启动一个作业的实例，分离了作业的执行和作业周围的状态。JobDetail 对象储存作业的侦听器、群组、数据映射、描述以及作业的其他属性。
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            //作业执行期间获取数据
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            JobDataMap dataMap1 = context.MergedJobDataMap;
            Console.WriteLine(dataMap.GetString(("jobSays")));
            Console.WriteLine(dataMap1["myFloatValue"]);

            Console.WriteLine("任务二调度成功" + ac + "次");
            ac++;
        }
    }

    /// <summary>
    /// AOP配置
    /// </summary>
    public class MyAopListener : IJobListener
    {
        public string Name => "hello czs";

        /// <summary>
        ///不知道做什么的
        /// </summary>
        /// <param name="context"></param>
        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            
        }

        /// <summary>
        /// was about to 将要执行
        /// </summary>
        /// <param name="context"></param>
        public void JobToBeExecuted(IJobExecutionContext context)
        {
            Console.WriteLine("将要执行前");
        }

        /// <summary>
        /// 执行后
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobException"></param>
        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            Console.WriteLine("执行完成后");
        }
    }
}
