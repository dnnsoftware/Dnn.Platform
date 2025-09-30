// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.ClientDependency
{
    using System.Web;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.ResourceManager;

    public static class Extensions
    {
        /// <summary>
        /// Registers a font resource by its path.
        /// </summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="fontPath">The path to the font resource to register.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterFont(this IClientResourceController controller, string fontPath, bool checkIfExists = false)
        {
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(fontPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(fontPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HttpContext.Current.Server.MapPath(fontPath);
                if (!System.IO.File.Exists(physicalPath))
                {
                    return;
                }
            }

            controller.CreateFont().FromSrc(fontPath).Register();
        }

        /// <summary>
        /// Registers a script resource by its path.
        /// </summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="scriptPath">The path to the script resource to register.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterScript(this IClientResourceController controller, string scriptPath, bool checkIfExists = false)
        {
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(scriptPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(scriptPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HttpContext.Current.Server.MapPath(scriptPath);
                if (!System.IO.File.Exists(physicalPath))
                {
                    return;
                }
            }

            controller.CreateScript().FromSrc(scriptPath).Register();
        }

        /// <summary>
        /// Registers a script resource by its path.
        /// </summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="scriptPath">The path to the script resource to register.</param>
        /// <param name="priority">The priority for loading the script.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterScript(this IClientResourceController controller, string scriptPath, FileOrder.Js priority, bool checkIfExists = false)
        {
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(scriptPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(scriptPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HttpContext.Current.Server.MapPath(scriptPath);
                if (!System.IO.File.Exists(physicalPath))
                {
                    return;
                }
            }

            controller.CreateScript()
                .FromSrc(scriptPath)
                .SetPriority((int)priority)
                .Register();
        }

        /// <summary>
        /// Registers a stylesheet resource by its path.
        /// </summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="stylesheetPath">The path to the stylesheet resource to register.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterStylesheet(this IClientResourceController controller, string stylesheetPath, bool checkIfExists = false)
        {
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(stylesheetPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(stylesheetPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HttpContext.Current.Server.MapPath(stylesheetPath);
                if (!System.IO.File.Exists(physicalPath))
                {
                    return;
                }
            }

            controller.CreateStylesheet().FromSrc(stylesheetPath).Register();
        }

        /// <summary>
        /// Registers a stylesheet resource by its path.
        /// </summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="stylesheetPath">The path to the stylesheet resource to register.</param>
        /// <param name="priority">The priority for loading the stylesheet.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterStylesheet(this IClientResourceController controller, string stylesheetPath, FileOrder.Css priority, bool checkIfExists = false)
        {
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(stylesheetPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(stylesheetPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HttpContext.Current.Server.MapPath(stylesheetPath);
                if (!System.IO.File.Exists(physicalPath))
                {
                    return;
                }
            }

            controller.CreateStylesheet()
                .FromSrc(stylesheetPath)
                .SetPriority((int)priority)
                .Register();
        }
    }
}
