import requests
from utils.AuthV3Util import addAuthParams
import xlrd
import xlwt
import os
import json
import time
from xlutils.copy import copy
import traceback

# 您的应用ID
APP_KEY = '521df84d17236059'
# 您的应用密钥
APP_SECRET = '5p6J4lUbQFzTw9h9oUeZhHrR9M7o7z4t'

def createRequest(source, vocab_id, lang_from, lang_to):
    '''
    note: 将下列变量替换为需要请求的参数
    '''
    data = {'q': source, 'from': lang_from, 'to': lang_to, 'vocabId': vocab_id}
    addAuthParams(APP_KEY, APP_SECRET, data)
    header = {'Content-Type': 'application/x-www-form-urlencoded'}
    res = doCall('https://openapi.youdao.com/api', header, data, 'post')
    jsonDict = json.loads(str(res.content, 'utf-8'));
    if jsonDict['errorCode'] == '0':
        return True, jsonDict['translation']
    else:
        # print(str(res.content, 'utf-8'))
        return False, ''
 

def doCall(url, header, params, method):
    if 'get' == method:
        return requests.get(url, params)
    elif 'post' == method:
        return requests.post(url, params, header)
    
def translate_excel(excel_file, dest_excel_file, lang_from, lang_to):
    print(dest_excel_file)

    # 打开 Excel 文件
    workbook = xlrd.open_workbook(excel_file)
    worksheet = workbook.sheet_by_index(0)

    # 打开现有的xls文件
    destWB = xlrd.open_workbook(dest_excel_file)
    destWS = destWB.sheet_by_index(0)
    
    wb = copy(destWB)
    ws = wb.get_sheet(0)
    ws.write(0, 0, 'ID')
    ws.write(0, 1, 'Str')
    ws.write(0, 2, 'Translated')

    try:
        total_rows = worksheet.nrows - 1  # 总行数，减去标题行
        row_index = 1
        while(row_index <= total_rows):
            if destWS.nrows < row_index + 1 or destWS.ncols < 3 or destWS.cell_value(row_index, 2) != 'true':
                original_text = worksheet.cell_value(row_index, 1)
                success, translated_text = createRequest(original_text, worksheet.cell_value(row_index, 0), lang_from, lang_to)
                if success:
                   print(f"Processing {row_index}/{total_rows} ({(row_index / total_rows) * 100:.2f}%) {dest_excel_file}")
                   ws.write(row_index, 2, 'true')
                   ws.write(row_index, 0, worksheet.cell_value(row_index, 0))
                   ws.write(row_index, 1, translated_text)
                   row_index = row_index + 1
                else:
                   time.sleep(1)
            else:
                row_index = row_index + 1
        # 保存新的 Excel 文件
        wb.save(dest_excel_file)
        print(f"Translation complete. File saved as {dest_excel_file}")
    except Exception as e:
        # 保存新的 Excel 文件
        wb.save(dest_excel_file)
        traceback.print_exc();
    

# 读取 TranslationRules.txt 文件并执行翻译任务
def main():
    with open("TranslationRules.txt", "r", encoding='utf-8') as file:
        for line in file:
            params = line.strip().split(',')
            excel_file = os.path.join('../..', 'Datas', params[0])
            dest_excel_file = os.path.join('../..', 'Datas',params[1])
            translate_excel(excel_file, dest_excel_file, params[2], params[3])

if __name__ == "__main__":
    main()

