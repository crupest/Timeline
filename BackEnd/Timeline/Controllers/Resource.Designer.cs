﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Timeline.Controllers {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.Controllers.Resource", typeof(Resource).Assembly);
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
        ///   Looks up a localized string similar to Can&apos;t get user id..
        /// </summary>
        internal static string ExceptionNoUserId {
            get {
                return ResourceManager.GetString("ExceptionNoUserId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t get username..
        /// </summary>
        internal static string ExceptionNoUsername {
            get {
                return ResourceManager.GetString("ExceptionNoUsername", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have no permission to access this..
        /// </summary>
        internal static string MessageForbid {
            get {
                return ResourceManager.GetString("MessageForbid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can&apos;t do this unless you are administrator..
        /// </summary>
        internal static string MessageForbidNotAdministrator {
            get {
                return ResourceManager.GetString("MessageForbidNotAdministrator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can&apos;t do this unless you are administrator or resource owner..
        /// </summary>
        internal static string MessageForbidNotAdministratorOrOwner {
            get {
                return ResourceManager.GetString("MessageForbidNotAdministratorOrOwner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Image is not a square..
        /// </summary>
        internal static string MessageImageBadSize {
            get {
                return ResourceManager.GetString("MessageImageBadSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Image decode failed..
        /// </summary>
        internal static string MessageImageDecodeFailed {
            get {
                return ResourceManager.GetString("MessageImageDecodeFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specified image format does not match the actual one ..
        /// </summary>
        internal static string MessageImageFormatUnmatch {
            get {
                return ResourceManager.GetString("MessageImageFormatUnmatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown error happened to image..
        /// </summary>
        internal static string MessageImageUnknownError {
            get {
                return ResourceManager.GetString("MessageImageUnknownError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can&apos;t do this because it is the root user..
        /// </summary>
        internal static string MessageInvalidOperationOnRootUser {
            get {
                return ResourceManager.GetString("MessageInvalidOperationOnRootUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The old password is wrong..
        /// </summary>
        internal static string MessageOldPasswordWrong {
            get {
                return ResourceManager.GetString("MessageOldPasswordWrong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Operation succeeded..
        /// </summary>
        internal static string MessageOperationSucceeded {
            get {
                return ResourceManager.GetString("MessageOperationSucceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user specified by query param &quot;relate&quot; does not exist..
        /// </summary>
        internal static string MessageTimelineListQueryRelateNotExist {
            get {
                return ResourceManager.GetString("MessageTimelineListQueryRelateNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; is an unkown visibility in the query parameter &apos;visibility&apos;. .
        /// </summary>
        internal static string MessageTimelineListQueryVisibilityUnknown {
            get {
                return ResourceManager.GetString("MessageTimelineListQueryVisibilityUnknown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Username or password is invalid..
        /// </summary>
        internal static string MessageTokenCreateBadCredential {
            get {
                return ResourceManager.GetString("MessageTokenCreateBadCredential", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The token is of bad format. It might not be created by the server..
        /// </summary>
        internal static string MessageTokenVerifyBadFormat {
            get {
                return ResourceManager.GetString("MessageTokenVerifyBadFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Token has an old version. User might have update some info..
        /// </summary>
        internal static string MessageTokenVerifyOldVersion {
            get {
                return ResourceManager.GetString("MessageTokenVerifyOldVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The token is expired..
        /// </summary>
        internal static string MessageTokenVerifyTimeExpired {
            get {
                return ResourceManager.GetString("MessageTokenVerifyTimeExpired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User does not exist. Administrator might have deleted this user..
        /// </summary>
        internal static string MessageTokenVerifyUserNotExist {
            get {
                return ResourceManager.GetString("MessageTokenVerifyUserNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A user with given username already exists..
        /// </summary>
        internal static string MessageUsernameConflict {
            get {
                return ResourceManager.GetString("MessageUsernameConflict", resourceCulture);
            }
        }
    }
}
