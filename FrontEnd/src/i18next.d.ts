// import the original type declarations
import "react-i18next";
// import all namespaces (for the default language, only)
import admin from "./locales/en/admin.json";
import translation from "./locales/en/translation.json";

// react-i18next versions higher than 11.11.0
declare module "i18next" {
  // and extend them!
  interface CustomTypeOptions {
    // custom namespace type if you changed it
    defaultNS: "translation";
    // custom resources type
    resources: {
      admin: typeof admin;
      translation: typeof translation;
    };
  }
}
