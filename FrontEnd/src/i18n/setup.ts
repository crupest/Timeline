import i18n from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";

import backend from "./backend";

void i18n
  .use(LanguageDetector)
  .use(backend)
  .use(initReactI18next) // bind react-i18next to the instance
  .init({
    fallbackLng: false,
    lowerCaseLng: true,

    debug: process.env.NODE_ENV === "development",

    interpolation: {
      escapeValue: false, // not needed for react!!
    },

    // react i18next special options (optional)
    // override if needed - omit if ok with defaults
    /*
    react: {
      bindI18n: 'languageChanged',
      bindI18nStore: '',
      transEmptyNodeValue: '',
      transSupportBasicHtmlNodes: true,
      transKeepBasicHtmlNodesFor: ['br', 'strong', 'i'],
      useSuspense: true,
    }
    */
  });

if (module.hot) {
  module.hot.accept(
    [
      "./translations/en/index.json",
      "./translations/zh/index.json",
      "./translations/en/admin.json",
      "./translations/zh/admin.json",
    ],
    () => {
      void i18n.reloadResources();
    },
  );
}

export default i18n;

