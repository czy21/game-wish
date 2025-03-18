using Autofac;
using WishServer.Manager;
using WishServer.Service;

namespace WishServer
{
    public class ServiceRegister : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.IsAssignableTo<IMessageHandler>())
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.IsAssignableTo<IPlatformService>())
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
