import i18n, { BackendModule, ResourceKey } from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";

const backend: BackendModule = {
  type: "backend",
  async read(language, namespace, callback) {
    function error(message: string): void {
      callback(new Error(message), false);
    }

    function success(result: ResourceKey): void {
      callback(null, result);
    }

    const promise = (() => {
      if (namespace === "translation") {
        if (language === "en") {
          return import("./locales/en/translation.json");
        } else if (language === "zh") {
          return import("./locales/zh/translation.json");
        } else {
          error(`Language ${language} is not supported.`);
        }
      } else if (namespace === "admin") {
        if (language === "en") {
          return import("./locales/en/admin.json");
        } else if (language === "zh") {
          return import("./locales/zh/admin.json");
        } else {
          error(`Language ${language} is not supported.`);
        }
      } else {
        error(`Namespace ${namespace} is not supported.`);
      }
    })();

    if (promise) {
      success((await promise).default);
    }
  },
  init() {}, // eslint-disable-line @typescript-eslint/no-empty-function
  create() {}, // eslint-disable-line @typescript-eslint/no-empty-function
};

export const i18nPromise = i18n
  .use(LanguageDetector)
  .use(backend)
  .use(initReactI18next) // bind react-i18next to the instance
  .init({
    fallbackLng: false,
    lowerCaseLng: true,

    debug: import.meta.env.DEV,

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

if (import.meta.hot) {
  import.meta.hot.accept(
    [
      "./locales/en/translation.json",
      "./locales/zh/translation.json",
      "./locales/en/admin.json",
      "./locales/zh/admin.json",
    ],
    () => {
      void i18n.reloadResources();
    }
  );
}

export default i18n;
