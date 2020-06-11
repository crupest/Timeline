import 'regenerator-runtime';
import 'core-js/modules/es.promise';
import 'core-js/modules/es.array.iterator';
import 'pepjs';

import React from 'react';
import ReactDOM from 'react-dom';

import './index.sass';

import './i18n';

import App from './App';

ReactDOM.render(<App />, document.getElementById('app'));

if ('serviceWorker' in navigator) {
  // Use the window load event to keep the page load performant
  window.addEventListener('load', () => {
    void navigator.serviceWorker.register('/sw.js');
  });
}
