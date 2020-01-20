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
    internal class Exception {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Exception() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.Resources.Services.Exception", typeof(Exception).Assembly);
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
        ///   Looks up a localized string similar to Avartar is of bad format because {0}..
        /// </summary>
        internal static string AvatarFormatException {
            get {
                return ResourceManager.GetString("AvatarFormatException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to image is not a square, aka, width is not equal to height.
        /// </summary>
        internal static string AvatarFormatExceptionBadSize {
            get {
                return ResourceManager.GetString("AvatarFormatExceptionBadSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to failed to decode image, see inner exception.
        /// </summary>
        internal static string AvatarFormatExceptionCantDecode {
            get {
                return ResourceManager.GetString("AvatarFormatExceptionCantDecode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to unknown error.
        /// </summary>
        internal static string AvatarFormatExceptionUnknownError {
            get {
                return ResourceManager.GetString("AvatarFormatExceptionUnknownError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to image&apos;s actual mime type is not the specified one.
        /// </summary>
        internal static string AvatarFormatExceptionUnmatchedFormat {
            get {
                return ResourceManager.GetString("AvatarFormatExceptionUnmatchedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The password is wrong..
        /// </summary>
        internal static string BadPasswordException {
            get {
                return ResourceManager.GetString("BadPasswordException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The hashes password is of bad format. It might not be created by server..
        /// </summary>
        internal static string HashedPasswordBadFromatException {
            get {
                return ResourceManager.GetString("HashedPasswordBadFromatException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not of valid base64 format. See inner exception..
        /// </summary>
        internal static string HashedPasswordBadFromatExceptionNotBase64 {
            get {
                return ResourceManager.GetString("HashedPasswordBadFromatExceptionNotBase64", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Decoded hashed password is of length 0..
        /// </summary>
        internal static string HashedPasswordBadFromatExceptionNotLength0 {
            get {
                return ResourceManager.GetString("HashedPasswordBadFromatExceptionNotLength0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to See inner exception..
        /// </summary>
        internal static string HashedPasswordBadFromatExceptionNotOthers {
            get {
                return ResourceManager.GetString("HashedPasswordBadFromatExceptionNotOthers", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Salt length &lt; 128 bits..
        /// </summary>
        internal static string HashedPasswordBadFromatExceptionNotSaltTooShort {
            get {
                return ResourceManager.GetString("HashedPasswordBadFromatExceptionNotSaltTooShort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subkey length &lt; 128 bits..
        /// </summary>
        internal static string HashedPasswordBadFromatExceptionNotSubkeyTooShort {
            get {
                return ResourceManager.GetString("HashedPasswordBadFromatExceptionNotSubkeyTooShort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown format marker..
        /// </summary>
        internal static string HashedPasswordBadFromatExceptionNotUnknownMarker {
            get {
                return ResourceManager.GetString("HashedPasswordBadFromatExceptionNotUnknownMarker", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The token didn&apos;t pass verification because {0}..
        /// </summary>
        internal static string JwtUserTokenBadFormatException {
            get {
                return ResourceManager.GetString("JwtUserTokenBadFormatException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to id claim is not a number.
        /// </summary>
        internal static string JwtUserTokenBadFormatExceptionIdBadFormat {
            get {
                return ResourceManager.GetString("JwtUserTokenBadFormatExceptionIdBadFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to id claim does not exist.
        /// </summary>
        internal static string JwtUserTokenBadFormatExceptionIdMissing {
            get {
                return ResourceManager.GetString("JwtUserTokenBadFormatExceptionIdMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to other error, see inner exception for information.
        /// </summary>
        internal static string JwtUserTokenBadFormatExceptionOthers {
            get {
                return ResourceManager.GetString("JwtUserTokenBadFormatExceptionOthers", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to unknown error.
        /// </summary>
        internal static string JwtUserTokenBadFormatExceptionUnknown {
            get {
                return ResourceManager.GetString("JwtUserTokenBadFormatExceptionUnknown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to version claim is not a number..
        /// </summary>
        internal static string JwtUserTokenBadFormatExceptionVersionBadFormat {
            get {
                return ResourceManager.GetString("JwtUserTokenBadFormatExceptionVersionBadFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to version claim does not exist..
        /// </summary>
        internal static string JwtUserTokenBadFormatExceptionVersionMissing {
            get {
                return ResourceManager.GetString("JwtUserTokenBadFormatExceptionVersionMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The timeline with that name already exists..
        /// </summary>
        internal static string TimelineAlreadyExistException {
            get {
                return ResourceManager.GetString("TimelineAlreadyExistException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exception happened when add or remove member on timeline..
        /// </summary>
        internal static string TimelineMemberOperationException {
            get {
                return ResourceManager.GetString("TimelineMemberOperationException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exception happened when do operation {0} on the {1} member on timeline..
        /// </summary>
        internal static string TimelineMemberOperationExceptionDetail {
            get {
                return ResourceManager.GetString("TimelineMemberOperationExceptionDetail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timeline name is of bad format. If this is a personal timeline, it means the username is of bad format and inner exception should be a UsernameBadFormatException..
        /// </summary>
        internal static string TimelineNameBadFormatException {
            get {
                return ResourceManager.GetString("TimelineNameBadFormatException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timeline does not exist. If this is a personal timeline, it means the user does not exist and inner exception should be a UserNotExistException..
        /// </summary>
        internal static string TimelineNotExistException {
            get {
                return ResourceManager.GetString("TimelineNotExistException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The timeline post does not exist. You can&apos;t do operation on it..
        /// </summary>
        internal static string TimelinePostNotExistException {
            get {
                return ResourceManager.GetString("TimelinePostNotExistException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The use is not a member of the timeline..
        /// </summary>
        internal static string TimelineUserNotMemberException {
            get {
                return ResourceManager.GetString("TimelineUserNotMemberException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The username is of bad format..
        /// </summary>
        internal static string UsernameBadFormatException {
            get {
                return ResourceManager.GetString("UsernameBadFormatException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The username already exists..
        /// </summary>
        internal static string UsernameConfictException {
            get {
                return ResourceManager.GetString("UsernameConfictException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user does not exist..
        /// </summary>
        internal static string UserNotExistException {
            get {
                return ResourceManager.GetString("UserNotExistException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The token is of bad format, which means it may not be created by the server..
        /// </summary>
        internal static string UserTokenBadFormatException {
            get {
                return ResourceManager.GetString("UserTokenBadFormatException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The token is of bad version..
        /// </summary>
        internal static string UserTokenBadVersionException {
            get {
                return ResourceManager.GetString("UserTokenBadVersionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The token is expired because its expiration time has passed..
        /// </summary>
        internal static string UserTokenTimeExpireException {
            get {
                return ResourceManager.GetString("UserTokenTimeExpireException", resourceCulture);
            }
        }
    }
}
