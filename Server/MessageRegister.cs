using Autofac;
using WishServer.Manager;
using WishServer.Service;

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
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.IsAssignableTo<IPlatformService>())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
