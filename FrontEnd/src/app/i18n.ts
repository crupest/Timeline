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

    if (namespace !== "translation") {
      error("Namespace must be 'translation'.");
    }

    if (language === "en") {
      const res = (await import("./locales/en/translation")).default;
      success(res);
    } else if (language === "zh-cn" || language === "zh") {
      const res = (await import("./locales/zh/translation")).default;
      success(res);
    } else {
      error(`Language ${language} is not supported.`);
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
    ["./locales/en/translation", "./locales/zh/translation"],
    () => {
      void i18n.reloadResources();
    }
  );
}

export default i18n;
