﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Timeline.Resources.Services {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Exceptions {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Exceptions() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.Resources.Services.Exceptions", typeof(Exceptions).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A entity of type &quot;{0}&quot; already exists..
        /// </summary>
        internal static string EntityAlreadyExistError {
            get {
                return ResourceManager.GetString("EntityAlreadyExistError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The entity already exists..
        /// </summary>
        internal static string EntityAlreadyExistErrorDefault {
            get {
                return ResourceManager.GetString("EntityAlreadyExistErrorDefault", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The required entity of type &quot;{0}&quot; does not exist..
        /// </summary>
        internal static string EntityNotExistError {
            get {
                return ResourceManager.GetString("EntityNotExistError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The required entity does not exist..
        /// </summary>
        internal static string EntityNotExistErrorDefault {
            get {
                return ResourceManager.GetString("EntityNotExistErrorDefault", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request timeline name is &quot;{0}&quot;. If this is a personal timeline whose name starts with &apos;@&apos;, it means the user does not exist and inner exception should be a UserNotExistException..
        /// </summary>
        internal static string TimelineNotExistException {
            get {
                return ResourceManager.GetString("TimelineNotExistException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request timeline name is &quot;{0}&quot;.  Request timeline post id is &quot;{1}&quot;..
        /// </summary>
        internal static string TimelinePostNotExistException {
            get {
                return ResourceManager.GetString("TimelinePostNotExistException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request timeline name is &quot;{0}&quot;.  Request timeline post id is &quot;{1}&quot;. The post does not exist because it has been deleted..
        /// </summary>
        internal static string TimelinePostNotExistExceptionDeleted {
            get {
                return ResourceManager.GetString("TimelinePostNotExistExceptionDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request username is &quot;{0}&quot;. Request id is &quot;{1}&quot;..
        /// </summary>
        internal static string UserNotExistException {
            get {
                return ResourceManager.GetString("UserNotExistException", resourceCulture);
            }
        }
    }
}
