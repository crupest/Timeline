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
    internal class UserAvatarService {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UserAvatarService() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.Resources.Services.UserAvatarService", typeof(UserAvatarService).Assembly);
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
        ///   Looks up a localized string similar to Data of avatar is null..
        /// </summary>
        internal static string ArgumentAvatarDataNull {
            get {
                return ResourceManager.GetString("ArgumentAvatarDataNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type of avatar is null or empty..
        /// </summary>
        internal static string ArgumentAvatarTypeNullOrEmpty {
            get {
                return ResourceManager.GetString("ArgumentAvatarTypeNullOrEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Database corupted! One of type and data of a avatar is null but the other is not..
        /// </summary>
        internal static string DatabaseCorruptedDataAndTypeNotSame {
            get {
                return ResourceManager.GetString("DatabaseCorruptedDataAndTypeNotSame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Created an entry in user_avatars..
        /// </summary>
        internal static string LogCreateEntity {
            get {
                return ResourceManager.GetString("LogCreateEntity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Updated an entry in user_avatars..
        /// </summary>
        internal static string LogUpdateEntity {
            get {
                return ResourceManager.GetString("LogUpdateEntity", resourceCulture);
            }
        }
    }
}
