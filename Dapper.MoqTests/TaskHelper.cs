using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    public class TaskHelper
    {
        internal static object GetResultOfTask(object task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var genericTask = NonGenericTask.ForTask(task);
            return genericTask.GetValue();
        }

        private abstract class NonGenericTask
        {
            public abstract object GetValue();

            public static NonGenericTask ForTask(object task)
            {
                var taskType = task.GetType();
                var taskValueType = taskType.GetGenericArguments().Single();

                var genericTaskType = typeof(GenericTask<>).MakeGenericType(taskValueType);
                var genericTask = Activator.CreateInstance(genericTaskType, new[] { task });

                return (NonGenericTask)genericTask;
            }
        }

        private class GenericTask<T> : NonGenericTask
        {
            private readonly Task<T> task;

            public GenericTask(Task<T> task)
            {
                this.task = task;
            }

            public override object GetValue()
            {
                return this.task.Result;
            }
        }
    }
}
