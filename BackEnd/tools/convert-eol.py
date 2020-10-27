# This is a python script that converts all text source codes into
# CRLF (Windows line ending) eol format and UTF-8 with NO BOM encoding.

import glob
import os.path

project_root = os.path.relpath(os.path.join(os.path.dirname(__file__), '..'))


def convert(file_path):
    with open(file_path, 'r', encoding='utf-8') as open_file:
        content = open_file.read()

    #if there is BOM, remove BOM
    if content[0] == '\ufeff':
        content = content[1:]

    with open(file_path, 'w', encoding='utf-8', newline='\r\n') as open_file:
        open_file.write(content)


glob_list = [
    './nuget.config',
    '**/*.sln',
    '**/*.cs',
    '**/*.csproj',
    '**/appsettings*.json'
]

for glob_pattern in glob_list:
    for f in glob.glob(glob_pattern, recursive=True):
        print('Converting {}'.format(f))
        convert(f)

print('Done!!!')
