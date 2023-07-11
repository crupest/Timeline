import i18n, { BackendModule } from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";

const backend: BackendModule = {
  type: "backend",
  init() {
    /* do nothing */
  },
  // eslint-disable-next-line @typescript-eslint/no-misused-promises
  async read(language, namespace) {
    if (namespace === "translation") {
      if (language === "en") {
        return await import("./locales/en/translation.json");
      } else if (language === "zh") {
        return await import("./locales/zh/translation.json");
      } else {
        throw Error(`Language ${language} is not supported.`);
      }
    } else if (namespace === "admin") {
      if (language === "en") {
        return await import("./locales/en/admin.json");
      } else if (language === "zh") {
        return await import("./locales/zh/admin.json");
      } else {
        throw Error(`Language ${language} is not supported.`);
      }
    } else {
      throw Error(`Namespace ${namespace} is not supported.`);
    }
  },
};

export const i18nPromise = i18n
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
      "./locales/en/translation.json",
      "./locales/zh/translation.json",
      "./locales/en/admin.json",
      "./locales/zh/admin.json",
    ],
    () => {
      void i18n.reloadResources();
    },
  );
}

export default i18n;
