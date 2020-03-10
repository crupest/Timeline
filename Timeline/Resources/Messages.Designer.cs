﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Timeline.Resources {
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
    internal class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.Resources.Messages", typeof(Messages).Assembly);
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
        ///   Looks up a localized string similar to Body is too big. It can&apos;t be bigger than {0}..
        /// </summary>
        internal static string Common_Content_TooBig {
            get {
                return ResourceManager.GetString("Common_Content_TooBig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Actual body length is bigger than it in header..
        /// </summary>
        internal static string Common_Content_UnmatchedLength_Bigger {
            get {
                return ResourceManager.GetString("Common_Content_UnmatchedLength_Bigger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Actual body length is smaller than it in header..
        /// </summary>
        internal static string Common_Content_UnmatchedLength_Smaller {
            get {
                return ResourceManager.GetString("Common_Content_UnmatchedLength_Smaller", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have no permission to do the operation..
        /// </summary>
        internal static string Common_Forbid {
            get {
                return ResourceManager.GetString("Common_Forbid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are not the resource owner..
        /// </summary>
        internal static string Common_Forbid_NotSelf {
            get {
                return ResourceManager.GetString("Common_Forbid_NotSelf", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Header Content-Length is missing or of bad format..
        /// </summary>
        internal static string Common_Header_ContentLength_Missing {
            get {
                return ResourceManager.GetString("Common_Header_ContentLength_Missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Header Content-Length must not be 0..
        /// </summary>
        internal static string Common_Header_ContentLength_Zero {
            get {
                return ResourceManager.GetString("Common_Header_ContentLength_Zero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Header Content-Type is missing..
        /// </summary>
        internal static string Common_Header_ContentType_Missing {
            get {
                return ResourceManager.GetString("Common_Header_ContentType_Missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Header If-Non-Match is of bad format..
        /// </summary>
        internal static string Common_Header_IfNonMatch_BadFormat {
            get {
                return ResourceManager.GetString("Common_Header_IfNonMatch_BadFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Model is of bad format..
        /// </summary>
        internal static string Common_InvalidModel {
            get {
                return ResourceManager.GetString("Common_InvalidModel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user to set as member does not exist..
        /// </summary>
        internal static string TimelineCommon_MemberPut_NotExist {
            get {
                return ResourceManager.GetString("TimelineCommon_MemberPut_NotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A timeline with given name already exists..
        /// </summary>
        internal static string TimelineCommon_NameConflict {
            get {
                return ResourceManager.GetString("TimelineCommon_NameConflict", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The timeline with given name does not exist..
        /// </summary>
        internal static string TimelineCommon_NotExist {
            get {
                return ResourceManager.GetString("TimelineCommon_NotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown type of post content..
        /// </summary>
        internal static string TimelineController_ContentUnknownType {
            get {
                return ResourceManager.GetString("TimelineController_ContentUnknownType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data field is not a valid base64 string in image content..
        /// </summary>
        internal static string TimelineController_ImageContentDataNotBase64 {
            get {
                return ResourceManager.GetString("TimelineController_ImageContentDataNotBase64", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data field is not a valid image after base64 decoding in image content..
        /// </summary>
        internal static string TimelineController_ImageContentDataNotImage {
            get {
                return ResourceManager.GetString("TimelineController_ImageContentDataNotImage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data field is required for image content..
        /// </summary>
        internal static string TimelineController_ImageContentDataRequired {
            get {
                return ResourceManager.GetString("TimelineController_ImageContentDataRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user specified by query param &quot;relate&quot; does not exist..
        /// </summary>
        internal static string TimelineController_QueryRelateNotExist {
            get {
                return ResourceManager.GetString("TimelineController_QueryRelateNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; is an unkown visibility in the query parameter &apos;visibility&apos;. .
        /// </summary>
        internal static string TimelineController_QueryVisibilityUnknown {
            get {
                return ResourceManager.GetString("TimelineController_QueryVisibilityUnknown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Text field is required for text content..
        /// </summary>
        internal static string TimelineController_TextContentTextRequired {
            get {
                return ResourceManager.GetString("TimelineController_TextContentTextRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Username or password is invalid..
        /// </summary>
        internal static string TokenController_Create_BadCredential {
            get {
                return ResourceManager.GetString("TokenController_Create_BadCredential", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The token is of bad format. It might not be created by the server..
        /// </summary>
        internal static string TokenController_Verify_BadFormat {
            get {
                return ResourceManager.GetString("TokenController_Verify_BadFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Token has an old version. User might have update some info..
        /// </summary>
        internal static string TokenController_Verify_OldVersion {
            get {
                return ResourceManager.GetString("TokenController_Verify_OldVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The token is expired..
        /// </summary>
        internal static string TokenController_Verify_TimeExpired {
            get {
                return ResourceManager.GetString("TokenController_Verify_TimeExpired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User does not exist. Administrator might have deleted this user..
        /// </summary>
        internal static string TokenController_Verify_UserNotExist {
            get {
                return ResourceManager.GetString("TokenController_Verify_UserNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Image is not a square..
        /// </summary>
        internal static string UserAvatar_BadFormat_BadSize {
            get {
                return ResourceManager.GetString("UserAvatar_BadFormat_BadSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Image decode failed..
        /// </summary>
        internal static string UserAvatar_BadFormat_CantDecode {
            get {
                return ResourceManager.GetString("UserAvatar_BadFormat_CantDecode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Image format does not match the one in header..
        /// </summary>
        internal static string UserAvatar_BadFormat_UnmatchedFormat {
            get {
                return ResourceManager.GetString("UserAvatar_BadFormat_UnmatchedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user to operate on does not exist..
        /// </summary>
        internal static string UserCommon_NotExist {
            get {
                return ResourceManager.GetString("UserCommon_NotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Old password is wrong..
        /// </summary>
        internal static string UserController_ChangePassword_BadOldPassword {
            get {
                return ResourceManager.GetString("UserController_ChangePassword_BadOldPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can&apos;t set permission unless you are administrator..
        /// </summary>
        internal static string UserController_Patch_Forbid_Administrator {
            get {
                return ResourceManager.GetString("UserController_Patch_Forbid_Administrator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can&apos;t set password unless you are administrator. If you want to change password, use /userop/changepassword ..
        /// </summary>
        internal static string UserController_Patch_Forbid_Password {
            get {
                return ResourceManager.GetString("UserController_Patch_Forbid_Password", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can&apos;t set username unless you are administrator..
        /// </summary>
        internal static string UserController_Patch_Forbid_Username {
            get {
                return ResourceManager.GetString("UserController_Patch_Forbid_Username", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A user with given username already exists..
        /// </summary>
        internal static string UserController_UsernameConflict {
            get {
                return ResourceManager.GetString("UserController_UsernameConflict", resourceCulture);
            }
        }
    }
}
