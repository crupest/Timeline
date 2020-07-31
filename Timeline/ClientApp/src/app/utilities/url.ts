//copied from https://stackoverflow.com/questions/5999118/how-can-i-add-or-update-a-query-string-parameter
export function updateQueryString(
  key: string,
  value: undefined | string | null,
  url: string
): string {
  const re = new RegExp('([?&])' + key + '=.*?(&|#|$)(.*)', 'gi');
  let hash;

  if (re.test(url)) {
    if (typeof value !== 'undefined' && value !== null) {
      return url.replace(re, '$1' + key + '=' + value + '$2$3');
    } else {
      hash = url.split('#');
      url = hash[0].replace(re, '$1$3').replace(/(&|\?)$/, '');
      if (typeof hash[1] !== 'undefined' && hash[1] !== null) {
        url += '#' + hash[1];
      }
      return url;
    }
  } else {
    if (typeof value !== 'undefined' && value !== null) {
      const separator = url.includes('?') ? '&' : '?';
      hash = url.split('#');
      url = hash[0] + separator + key + '=' + value;
      if (typeof hash[1] !== 'undefined' && hash[1] !== null) {
        url += '#' + hash[1];
      }
      return url;
    } else {
      return url;
    }
  }
}

export function applyQueryParameters<T>(url: string, query: T): string {
  if (query == null) return url;

  for (const [key, value] of Object.entries(query)) {
    if (typeof value === 'string') url = updateQueryString(key, value, url);
    else if (typeof value === 'number')
      url = updateQueryString(key, String(value), url);
    else if (typeof value === 'boolean')
      url = updateQueryString(key, value ? 'true' : 'false', url);
    else if (value instanceof Date)
      url = updateQueryString(key, value.toISOString(), url);
    else {
      console.error('Unknown query parameter type. Param: ', value);
    }
  }
  return url;
}
