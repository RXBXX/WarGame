# -*- coding=utf-8 -*-
import xlrd
import warnings
from collections import OrderedDict
import json
import codecs
import importlib
 
warnings.filterwarnings("ignore")
 
def excel2json(excelPath, jsonPath, fileName):
    wb = xlrd.open_workbook('{excelPath}{fileName}.xls'.format(excelPath=excelPath, fileName=fileName))
 
    convert_list = []
    for sheetNo in range(0, len(wb.sheets())):
        sheetName = wb.sheet_by_index(sheetNo).name
        sh = wb.sheet_by_index(sheetNo)
        title = sh.row_values(0)
 
        for rownum in range(1, sh.nrows):
            rowvalue = sh.row_values(rownum)
            single = OrderedDict()
            for colnum in range(0, len(rowvalue)):
                single[title[colnum]] = rowvalue[colnum]
            convert_list.append(single)
 
        j = json.dumps(convert_list)
 
        with codecs.open('{jsonPath}{fileName}-{sheetName}.json'.format(jsonPath=jsonPath, fileName=fileName, sheetName=sheetName), "w", "utf-8") as f:
            f.write(j)
 
# Batch Test
excelPath = 'E:/WarGame/WarGame_Data/Datas/'
jsonPath = 'E:/WarGame/WarGame_Data/Jsons/'
fileName = '角色表'
excel2json(excelPath, jsonPath, fileName)