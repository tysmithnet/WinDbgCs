﻿using CsScriptManaged;
using DbgEngManaged;
using System;
using System.Text;

namespace CsScripts
{
    public class Module
    {
        /// <summary>
        /// The module name
        /// </summary>
        private SimpleCache<string> name;

        /// <summary>
        /// The image name
        /// </summary>
        private SimpleCache<string> imageName;

        /// <summary>
        /// The loaded image name
        /// </summary>
        private SimpleCache<string> loadedImageName;

        /// <summary>
        /// The symbol file name
        /// </summary>
        private SimpleCache<string> symbolFileName;

        /// <summary>
        /// The mapped image name
        /// </summary>
        private SimpleCache<string> mappedImageName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal Module(Process process, ulong id)
        {
            Id = id;
            Process = process;
            name = new SimpleCache<string>(() => GetName(DebugModname.Module));
            imageName = new SimpleCache<string>(() => GetName(DebugModname.Image));
            loadedImageName = new SimpleCache<string>(() => GetName(DebugModname.LoadedImage));
            symbolFileName = new SimpleCache<string>(() => GetName(DebugModname.SymbolFile));
            mappedImageName = new SimpleCache<string>(() => GetName(DebugModname.MappedImage));
        }

        /// <summary>
        /// Gets all modules for the current process.
        /// </summary>
        public static Module[] All
        {
            get
            {
                return Process.Current.Modules;
            }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public ulong Id { get; private set; }

        /// <summary>
        /// Gets the owning process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the offset (address location of module base).
        /// </summary>
        public ulong Offset
        {
            get
            {
                return Id;
            }
        }

        /// <summary>
        /// Gets the module name. This is usually just the file name without the extension. In a few cases,
        /// the module name differs significantly from the file name.
        /// </summary>
        public string Name
        {
            get
            {
                return name.Value;
            }
        }

        /// <summary>
        /// Gets the name of the image. This is the name of the executable file, including the extension.
        /// Typically, the full path is included in user mode but not in kernel mode.
        /// </summary>
        public string ImageName
        {
            get
            {
                return imageName.Value;
            }
        }

        /// <summary>
        /// Gets the name of the loaded image. Unless Microsoft CodeView symbols are present, this is the same as the image name.
        /// </summary>
        public string LoadedImageName
        {
            get
            {
                return loadedImageName.Value;
            }
        }

        /// <summary>
        /// Gets the name of the symbol file. The path and name of the symbol file. If no symbols have been loaded,
        /// this is the name of the executable file instead.
        /// </summary>
        public string SymbolFileName
        {
            get
            {
                return symbolFileName.Value;
            }
        }

        /// <summary>
        /// Gets the name of the mapped image. In most cases, this is NULL. If the debugger is mapping an image file
        /// (for example, during minidump debugging), this is the name of the mapped image.
        /// </summary>
        public string MappedImageName
        {
            get
            {
                return mappedImageName.Value;
            }
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Variable if found</returns>
        /// <exception cref="System.ArgumentException">Variable name contains wrong module name. Don't add it manually, it will be added automatically.</exception>
        public Variable GetVariable(string name)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(Process))
            {
                int moduleIndex = name.IndexOf('!');

                if (moduleIndex > 0)
                {
                    if (string.Compare(name.Substring(0, moduleIndex), Name, true) != 0)
                    {
                        throw new ArgumentException("Variable name contains wrong module name. Don't add it manually, it will be added automatically.");
                    }
                }
                else
                {
                    name = Name + "!" + name;
                }

                return Variable.FromName(name);
            }
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <param name="modname">The type of module name.</param>
        /// <returns>Read name</returns>
        private string GetName(DebugModname modname)
        {
            uint nameSize;
            StringBuilder sb = new StringBuilder(Constants.MaxFileName);

            Context.Symbols.GetModuleNameStringWide((uint)modname, 0xffffffff, Id, sb, (uint)sb.Capacity, out nameSize);
            return sb.ToString();
        }
    }
}
