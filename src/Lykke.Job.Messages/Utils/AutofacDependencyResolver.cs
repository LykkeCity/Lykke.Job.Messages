﻿using Autofac;
using JetBrains.Annotations;
using Lykke.Cqrs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Utils
{
    internal class AutofacDependencyResolver : IDependencyResolver
    {
        private readonly IComponentContext _context;

        public AutofacDependencyResolver([NotNull] IComponentContext kernel)
        {
            _context = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public object GetService(Type type)
        {
            return _context.Resolve(type);
        }

        public bool HasService(Type type)
        {
            return _context.IsRegistered(type);
        }
    }
}
