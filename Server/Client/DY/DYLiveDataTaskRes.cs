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
    }
}
