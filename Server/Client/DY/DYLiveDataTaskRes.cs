namespace WishServer.Client.DY
{
    public class DYLiveDataTaskRes
    {
        public int err_no { get; set; }
        public string err_msg { get; set; }
        public string logid { get; set; }
        public DYLiveDataTaskData data { get; set; }

    }

    public class DYLiveDataTaskData
    {
        public string taskid { set; get; }
        public int status { set; get; } // int, 取值：1 任务不存在 2任务未启动 3任务运行中
    }
}
