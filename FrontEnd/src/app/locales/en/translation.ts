import TranslationResource from "../scheme";

const translation: TranslationResource = {
  welcome: "Welcome!",
  search: "Search",
  loadFailReload: "Load failed, click <1>here</1> to reload.",
  serviceWorker: {
    availableOffline:
      "Timeline is now cached in your computer and you can use it offline. 🎉🎉🎉",
    upgradePrompt: "App is getting a new version!",
    upgradeNow: "Update Now",
    upgradeSuccess:
      "Congratulations! App update succeeded! Still you can use it offline. 🎉🎉🎉",
    externalActivatedPrompt:
      "A new version of app is activated. Please refresh the page. Or it may be broken.",
    reloadNow: "Refresh Now",
  },
  nav: {
    settings: "Settings",
    login: "Login",
    about: "About",
  },
  chooseImage: "Choose a image",
  loadImageError: "Failed to load image.",
  home: {
    go: "Go!",
    allTimeline: "All Timelines",
    joinTimeline: "Joined Timelines",
    ownTimeline: "Owned Timelines",
    offlinePrompt:
      "Oh oh, it seems you are offline. Here list some timelines cached locally. You can view them or click <1>here</1> to refresh.",
    createButton: "Create Timeline",
    createDialog: {
      title: "Create Timeline!",
      name: "Name",
      nameFormat:
        "Name must consist of only letter including non-English letter, digit, hyphen(-) and underline(_) and be no longer than 26.",
      badFormat: "Bad format.",
      noEmpty: "Empty is not allowed.",
      tooLong: "Too long.",
    },
  },
  operationDialog: {
    retry: "Retry",
    nextStep: "Next",
    previousStep: "Previous",
    confirm: "Confirm",
    cancel: "Cancel",
    ok: "OK!",
    processing: "Processing...",
    success: "Success!",
    error: "An error occured.",
  },
  timeline: {
    messageCantSee: "Sorry, you are not allowed to see this timeline.😅",
    userNotExist: "The user does not exist!",
    timelineNotExist: "The timeline does not exist!",
    manage: "Manage",
    memberButton: "Member",
    send: "Send",
    deletePostFailed: "Failed to delete post.",
    sendPostFailed: "Failed to send post.",
    visibility: {
      public: "public to everyone",
      register: "only registed people can see",
      private: "only members can see",
    },
    visibilityTooltip: {
      public:
        "Everyone including those without accounts can see content of the timeline.",
      register:
        "Only those who have an account and logined can see content of the timeline.",
      private: "Only members of this timeline can see content of the timeline.",
    },
    dialogChangeProperty: {
      title: "Change Timeline Properties",
      titleField: "Title",
      visibility: "Visibility",
      description: "Description",
    },
    member: {
      alreadyMember: "The user is already a member.",
      add: "Add",
      remove: "Remove",
    },
    manageItem: {
      nickname: "Nickname",
      avatar: "Avatar",
      property: "Timeline Property",
      member: "Timeline Member",
      delete: "Delete Timeline",
    },
    deleteDialog: {
      title: "Delete Timeline",
      inputPrompt:
        "This is a dangerous action. If you are sure to delete timeline<1>{{name}}</1>, please input its name below and click confirm button.",
      notMatch: "Name does not match.",
    },
    postSyncState: {
      syncing: "Syncing",
      synced: "Synced",
      offline: "Offline",
    },
    post: {
      deleteDialog: {
        title: "Confirm Delete",
        prompt:
          "Are you sure to delete the post? This operation is not recoverable.",
      },
    },
  },
  user: {
    username: "username",
    password: "password",
    login: "login",
    rememberMe: "Remember Me",
    welcomeBack: "Welcome back!",
    verifyTokenFailed: "User login info is expired. Please login again!",
    verifyTokenFailedNetwork:
      "Verifying user login info failed. Please check your network and refresh page!",
  },
  login: {
    emptyUsername: "Username can't be empty.",
    emptyPassword: "Password can't be empty.",
    badCredential: "Username or password is invalid.",
    alreadyLogin: "Already login! Redirect to home page in 3s!",
  },
  userPage: {
    dialogChangeNickname: {
      title: "Change Nickname",
      inputLabel: "New nickname",
    },
    dialogChangeAvatar: {
      title: "Change Avatar",
      previewImgAlt: "preview",
      prompt: {
        select: "Please select a picture.",
        crop: "Please crop the picture.",
        processingCrop: "Cropping picture...",
        uploading: "Uploading...",
        preview: "Please preview avatar",
      },
      upload: "upload",
    },
  },
  settings: {
    subheaders: {
      account: "Account",
      customization: "Customization",
    },
    languagePrimary: "Choose display language.",
    languageSecondary:
      "You language preference will be saved locally. Next time you visit this page, last language option will be used.",
    changePassword: "Change account's password.",
    logout: "Log out this account.",
    gotoSelf:
      "Click here to go to timeline of myself to change nickname and avatar.",
    dialogChangePassword: {
      title: "Change Password",
      prompt:
        "You are changing your password. You need to input the correct old password. After change, you need to login again and all old login will be invalid.",
      inputOldPassword: "Old password",
      inputNewPassword: "New password",
      inputRetypeNewPassword: "Retype new password",
      errorEmptyOldPassword: "Old password can't be empty.",
      errorEmptyNewPassword: "New password can't be empty.",
      errorRetypeNotMatch: "Password retyped does not match.",
    },
    dialogConfirmLogout: {
      title: "Confirm Logout",
      prompt:
        "Are you sure to log out? All cached data in the browser will be deleted.",
    },
  },
  about: {
    author: {
      title: "Site Developer",
      fullname: "Fullname: ",
      nickname: "Nickname: ",
      introduction: "Introduction: ",
      introductionContent: "A programmer coding based on coincidence",
      links: "Links: ",
    },
    site: {
      title: "Site Information",
      content:
        "The name of this site is <1>Timeline</1>, which is a Web App with <3>timeline</3> as its core concept. Its frontend and backend are both developed by <5>me</5>, and open source on GitHub. It is relatively easy to deploy it on your own server, which is also one of my goals. Welcome to comment anything in GitHub repository.",
      repo: "GitHub Repo",
    },
    credits: {
      title: "Credits",
      content:
        "Timeline is works standing on shoulders of gaints. Special appreciation for many open source projects listed below or not. Related licenses could be found in GitHub repository.",
      frontend: "Frontend: ",
      backend: "Backend: ",
    },
  },
  admin: {
    title: "admin",
  },
};

export default translation;
