﻿// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.ComponentModel.Composition
{
    public static partial class PartInitializer
    {
        /// <summary>
        ///     Will satisfy the imports on a object instance based on a <see cref="CompositionContainer"/>
        ///     registered with the <see cref="CompositionHost"/>. By default if no <see cref="CompositionContainer"/>
        ///     is registered the first time this is called it will be initialized to a catalog
        ///     that contains all the assemblies loaded by the initial application XAP.
        /// </summary>
        /// <param name="instance">
        ///     Object instance that contains <see cref="ImportAttribute"/>s that need to be satisfied.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="instance"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="instance"/> contains <see cref="ExportAttribute"/>s applied on its type.
        /// </exception>
        /// <exception cref="ChangeRejectedException">
        ///     One or more of the imports on the object instance could not be satisfied.
        /// </exception>
        /// <exception cref="CompositionException">
        ///     One or more of the imports on the object instance caused an error while composing.
        /// </exception>
        public static void SatisfyImports(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            var batch = new CompositionBatch();

            var attributedPart = batch.AddPart(instance);

            if (attributedPart.ExportDefinitions.Any())
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                        Strings.ArgumentException_TypeHasExports, instance.GetType().FullName), "instance");
            }

            CompositionContainer container = null;

            // Ignoring return value because we don't need to know if we created it or not
            CompositionHost.TryGetOrCreateContainer(_createContainer, out container);

            container.Compose(batch);
        }

        private static Func<CompositionContainer> _createContainer = CreateCompositionContainer;
        private static CompositionContainer CreateCompositionContainer()
        {
            var assemblyCatalogs = GetAssemblyList()
                .Select<Assembly, ComposablePartCatalog>(assembly => new AssemblyCatalog(assembly));

            var aggCatalog = new AggregateCatalog(assemblyCatalogs);

            return new CompositionContainer(aggCatalog);
        }
    }
}