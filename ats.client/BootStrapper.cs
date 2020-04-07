using ats.client.Helpers;
using ats.client.Model;
using ats.client.ViewModel;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ats.client
{
    internal static class BootStrapper
    {
        public static IContainer Container { get; private set; }

        internal static void Boot()
        {
            InitializeContainer();
        }

        private static void InitializeContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacClientModule>();
            Container = builder.Build();
        }
    }

    internal class AutofacClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Shell>().SingleInstance();
            builder.RegisterType<FaceDataModel>().InstancePerLifetimeScope();
            builder.RegisterType<AtsViewModel>().InstancePerLifetimeScope();
        }
    }
}
