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
        ///   Looks up a localized string similar to Color is not valid. {0}.
        /// </summary>
        internal static string ExceptionColorInvalid {
            get {
                return ResourceManager.GetString("ExceptionColorInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data list can&apos;t be empty..
        /// </summary>
        internal static string ExceptionDataListEmpty {
            get {
                return ResourceManager.GetString("ExceptionDataListEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data list can&apos;t be null..
        /// </summary>
        internal static string ExceptionDataListNull {
            get {
                return ResourceManager.GetString("ExceptionDataListNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data list count can&apos;t be bigger than 100..
        /// </summary>
        internal static string ExceptionDataListTooLarge {
            get {
                return ResourceManager.GetString("ExceptionDataListTooLarge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This timeline name is neither a valid personal timeline name nor a valid ordinary timeline name. {0}.
        /// </summary>
        internal static string ExceptionGeneralTimelineNameBadFormat {
            get {
                return ResourceManager.GetString("ExceptionGeneralTimelineNameBadFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Image validation failed..
        /// </summary>
        internal static string ExceptionPostDataImageInvalid {
            get {
                return ResourceManager.GetString("ExceptionPostDataImageInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to It is not a valid utf-8 sequence..
        /// </summary>
        internal static string ExceptionPostDataNotValidUtf8 {
            get {
                return ResourceManager.GetString("ExceptionPostDataNotValidUtf8", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unsupported content type..
        /// </summary>
        internal static string ExceptionPostDataUnsupportedType {
            get {
                return ResourceManager.GetString("ExceptionPostDataUnsupportedType", resourceCulture);
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
        
        /// <summary>
        ///   Looks up a localized string similar to A personal timeline for user with username={0} is created automatically..
        /// </summary>
        internal static string LogPersonalTimelineAutoCreate {
            get {
                return ResourceManager.GetString("LogPersonalTimelineAutoCreate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A member(user id={0}) is added to timeline(id={1})..
        /// </summary>
        internal static string LogTimelineAddMember {
            get {
                return ResourceManager.GetString("LogTimelineAddMember", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A timeline is created with name={0}, id={1}..
        /// </summary>
        internal static string LogTimelineCreate {
            get {
                return ResourceManager.GetString("LogTimelineCreate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A timeline(id={0}) is deleted..
        /// </summary>
        internal static string LogTimelineDelete {
            get {
                return ResourceManager.GetString("LogTimelineDelete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A timeline(id={0}) post(id={1}) is created..
        /// </summary>
        internal static string LogTimelinePostCreated {
            get {
                return ResourceManager.GetString("LogTimelinePostCreated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A timeline(id={0}) post(id={1}) is deleted..
        /// </summary>
        internal static string LogTimelinePostDeleted {
            get {
                return ResourceManager.GetString("LogTimelinePostDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A timeline(id={0}) post(id={1}) is updated..
        /// </summary>
        internal static string LogTimelinePostUpdated {
            get {
                return ResourceManager.GetString("LogTimelinePostUpdated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A member(user id={0}) is removed from timeline(id={1})..
        /// </summary>
        internal static string LogTimelineRemoveMember {
            get {
                return ResourceManager.GetString("LogTimelineRemoveMember", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timeline with id={0} is updated..
        /// </summary>
        internal static string LogTimelineUpdated {
            get {
                return ResourceManager.GetString("LogTimelineUpdated", resourceCulture);
            }
        }
    }
}
