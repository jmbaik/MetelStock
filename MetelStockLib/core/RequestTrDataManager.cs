using AxKHOpenAPILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetelStockLib.core
{
    public class RequestTrDataManager
    {
        private static RequestTrDataManager requestTrDataManager;

        Queue<Task> requestTaskQueue = new Queue<Task>(); //TR요청 Task Queue

        Thread taskWorker; //Task Queue에 쌓인 TR요청을 순차적으로 처리하는 쓰레드

        public int REQUEST_DELAY = 610;

        private RequestTrDataManager()
        {
            taskWorker = new Thread(delegate ()
            {
                while (true)
                {
                    try
                    {
                        while (requestTaskQueue.Count > 0)
                        {
                            requestTaskQueue.Dequeue().RunSynchronously();
                            Thread.Sleep(REQUEST_DELAY);
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
            });
        }

        public static RequestTrDataManager GetInstance()
        {
            if (requestTrDataManager == null)
                requestTrDataManager = new RequestTrDataManager();

            return requestTrDataManager;
        }

        public void Run()
        {
            taskWorker.Start();
        }

        public void RequestTrData(Task task) //Task 형식으로 TR Data Request를 전달받는다.
        {
            requestTaskQueue.Enqueue(task);
        }

        public void RequestTrDataOnFirst(Task task) //Task 형식으로 TR Data Request를 전달받는다.
        {
            Queue<Task> tempQueue = requestTaskQueue;
            requestTaskQueue.Clear();
            requestTaskQueue.Enqueue(task);
            foreach (Task _task in tempQueue)
            {
                requestTaskQueue.Enqueue(_task);
            }
        }
    }
}
