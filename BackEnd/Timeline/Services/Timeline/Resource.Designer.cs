﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Timeline.Services.Timeline {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.Services.Timeline.Resource", typeof(Resource).Assembly);
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
        ///   Looks up a localized string similar to Timeline with given constraints already exist..
        /// </summary>
        internal static string ExceptionTimelineAlreadyExist {
            get {
                return ResourceManager.GetString("ExceptionTimelineAlreadyExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timeline name is of bad format. {0}.
        /// </summary>
        internal static string ExceptionTimelineNameBadFormat {
            get {
                return ResourceManager.GetString("ExceptionTimelineNameBadFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested timeline does not exist..
        /// </summary>
        internal static string ExceptionTimelineNotExist {
            get {
                return ResourceManager.GetString("ExceptionTimelineNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Requested timeline post does not exist because {0}..
        /// </summary>
        internal static string ExceptionTimelinePostNoExist {
            get {
                return ResourceManager.GetString("ExceptionTimelinePostNoExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to it is deleted.
        /// </summary>
        internal static string ExceptionTimelinePostNoExistReasonDeleted {
            get {
                return ResourceManager.GetString("ExceptionTimelinePostNoExistReasonDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to it has not been created.
        /// </summary>
        internal static string ExceptionTimelinePostNoExistReasonNotCreated {
            get {
                return ResourceManager.GetString("ExceptionTimelinePostNoExistReasonNotCreated", resourceCulture);
            }
        }
    }
}