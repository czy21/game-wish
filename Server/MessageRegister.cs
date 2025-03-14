using Autofac;
using WishServer.Manager;

namespace WishServer
{
    public class MessageRegister : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.IsAssignableTo<IMessageHandler>())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
