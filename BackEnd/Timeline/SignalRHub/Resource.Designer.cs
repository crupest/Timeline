﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Timeline.SignalRHub {
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
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.SignalRHub.Resource", typeof(Resource).Assembly);
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
        ///   Looks up a localized string similar to Connection {0} is added to group {1} to listen to timeline post change..
        /// </summary>
        internal static string LogSubscribeTimelinePostChange {
            get {
                return ResourceManager.GetString("LogSubscribeTimelinePostChange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove connection {0} from group {1} to stop listening to timeline post change..
        /// </summary>
        internal static string LogUnsubscribeTimelinePostChange {
            get {
                return ResourceManager.GetString("LogUnsubscribeTimelinePostChange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are forbidden to do this..
        /// </summary>
        internal static string MessageForbidden {
            get {
                return ResourceManager.GetString("MessageForbidden", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timeline name is illegal..
        /// </summary>
        internal static string MessageTimelineNameInvalid {
            get {
                return ResourceManager.GetString("MessageTimelineNameInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timeline does not exist..
        /// </summary>
        internal static string MessageTimelineNotExist {
            get {
                return ResourceManager.GetString("MessageTimelineNotExist", resourceCulture);
            }
        }
    }
}
