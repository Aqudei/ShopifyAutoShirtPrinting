import json
import requests

headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8",
    "Sec-Ch-Ua": 'Chromium";v="116", "Not)A;Brand";v="24", "Brave";v="116"',
    'Accept-Encoding': 'gzip, deflate, br',
    'Accept-Language': 'en-US,en;q=0.5',
    'Cache-Control': 'max-age=0',
    "Upgrade-Insecure-Requests": "1",
    "Sec-Ch-Ua-Platform": "Windows",
    "Sec-Ch-Ua-Mobile": "?0",
    "Sec-Fetch-Dest": "document",
    "Sec-Fetch-Mode": "navigate",
    "Sec-Gpc": "1",
    "Sec-Fetch-User": "?1",
    "Sec-Fetch-Site": "none",
}
# Replace with your QuickConnect URL
quickconnect_url = "https://lonelykidsclub.sg4.quickconnect.to"

username = "aqudei"
password = "Espelimbergo"

session = requests.Session()

def info():

    resource = '/webapi/query.cgi'

    params = {
        'api': 'SYNO.API.Info',
        'version': '1',
        'method': 'query',
        'query': 'all'
    }

    url = f'{quickconnect_url}{resource}'

    response = session.get(
        url, params=params, headers=headers, cookies={"type": "tunnel"})

    with open("./info.json", 'wt') as outfile:
        outfile.write(json.dumps(response.json()))


def filestation_info():
    """
    docstring
    """
    resource = '/webapi/entry.cgi'

    params = {
        'api': 'SYNO.FileStation.Info',
        'method': 'get'
    }

    url = f'{quickconnect_url}{resource}'

    response = session.get(
        url, params=params, headers=headers, cookies={"type": "tunnel"})

    with open("./info.json", 'wt') as outfile:
        outfile.write(json.dumps(response.json()))


def login(username, password):
    """
    docstring
    """

    resource = '/webapi/entry.cgi'

    params = {
        'api': 'SYNO.API.Auth',
        'method': 'login',
        'version': 1,
        'account': username,
        'passwd': password,
        'session': 'FileStation',
        'format': 'cookie'
    }

    url = f'{quickconnect_url}{resource}'

    response = session.get(
        url, params=params, headers=headers, cookies={"type": "tunnel"})
    
    print(response.text)

    import pdb
    pdb.set_trace()
    with open("./auth.json", 'wt') as outfile:
        outfile.write(json.dumps(response.json()))


login(username, password)
