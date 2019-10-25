﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Timeline.Resources.Controllers {
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
    internal class UserAvatarController {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UserAvatarController() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.Resources.Controllers.UserAvatarController", typeof(UserAvatarController).Assembly);
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
        ///   Looks up a localized string similar to Normal user can&apos;t delete other&apos;s avatar..
        /// </summary>
        internal static string ErrorDeleteForbid {
            get {
                return ResourceManager.GetString("ErrorDeleteForbid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User does not exist..
        /// </summary>
        internal static string ErrorDeleteUserNotExist {
            get {
                return ResourceManager.GetString("ErrorDeleteUserNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User does not exist..
        /// </summary>
        internal static string ErrorGetUserNotExist {
            get {
                return ResourceManager.GetString("ErrorGetUserNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Image is not a square..
        /// </summary>
        internal static string ErrorPutBadFormatBadSize {
            get {
                return ResourceManager.GetString("ErrorPutBadFormatBadSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Decoding image failed..
        /// </summary>
        internal static string ErrorPutBadFormatCantDecode {
            get {
                return ResourceManager.GetString("ErrorPutBadFormatCantDecode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Image format is not the one in header..
        /// </summary>
        internal static string ErrorPutBadFormatUnmatchedFormat {
            get {
                return ResourceManager.GetString("ErrorPutBadFormatUnmatchedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Normal user can&apos;t change other&apos;s avatar..
        /// </summary>
        internal static string ErrorPutForbid {
            get {
                return ResourceManager.GetString("ErrorPutForbid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User does not exist..
        /// </summary>
        internal static string ErrorPutUserNotExist {
            get {
                return ResourceManager.GetString("ErrorPutUserNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown AvatarDataException.ErrorReason value..
        /// </summary>
        internal static string ExceptionUnknownAvatarFormatError {
            get {
                return ResourceManager.GetString("ExceptionUnknownAvatarFormatError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempt to delete a avatar of other user as a non-admin failed..
        /// </summary>
        internal static string LogDeleteForbid {
            get {
                return ResourceManager.GetString("LogDeleteForbid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempt to delete a avatar of a non-existent user failed..
        /// </summary>
        internal static string LogDeleteNotExist {
            get {
                return ResourceManager.GetString("LogDeleteNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Succeed to delete a avatar of a user..
        /// </summary>
        internal static string LogDeleteSuccess {
            get {
                return ResourceManager.GetString("LogDeleteSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempt to get a avatar with If-None-Match in bad format..
        /// </summary>
        internal static string LogGetBadIfNoneMatch {
            get {
                return ResourceManager.GetString("LogGetBadIfNoneMatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Returned full data for a get avatar attempt..
        /// </summary>
        internal static string LogGetReturnData {
            get {
                return ResourceManager.GetString("LogGetReturnData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Returned NotModify for a get avatar attempt..
        /// </summary>
        internal static string LogGetReturnNotModify {
            get {
                return ResourceManager.GetString("LogGetReturnNotModify", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempt to get a avatar of a non-existent user failed..
        /// </summary>
        internal static string LogGetUserNotExist {
            get {
                return ResourceManager.GetString("LogGetUserNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempt to put a avatar of other user as a non-admin failed..
        /// </summary>
        internal static string LogPutForbid {
            get {
                return ResourceManager.GetString("LogPutForbid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Succeed to put a avatar of a user..
        /// </summary>
        internal static string LogPutSuccess {
            get {
                return ResourceManager.GetString("LogPutSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempt to put a avatar of a bad format failed..
        /// </summary>
        internal static string LogPutUserBadFormat {
            get {
                return ResourceManager.GetString("LogPutUserBadFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempt to put a avatar of a non-existent user failed..
        /// </summary>
        internal static string LogPutUserNotExist {
            get {
                return ResourceManager.GetString("LogPutUserNotExist", resourceCulture);
            }
        }
    }
}
