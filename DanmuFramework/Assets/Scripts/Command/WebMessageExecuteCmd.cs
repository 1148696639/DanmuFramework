using QFramework;

namespace Command
{
    public class WebMessageExecuteCmd : AbstractCommand
    {
        private readonly object m_Data;
        private readonly string m_Method;

        public WebMessageExecuteCmd(string method, object data)
        {
            m_Method = method;
            m_Data = data;
        }

        protected override void OnExecute()
        {
            this.SendEvent(new WebMessageExecuteEvent(m_Method, m_Data));
        }
    }
}