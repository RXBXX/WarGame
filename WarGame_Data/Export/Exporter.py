import xlrd
import json
import os

def excel_to_json(excel_file, json_file):
    # 打开 Excel 文件
    workbook = xlrd.open_workbook(excel_file)
    # 获取第一个工作表
    worksheet = workbook.sheet_by_index(0)

    # 初始化一个空的 JSON 数据列表
    data = []

    # 遍历工作表的每一行，将数据转换为字典格式，并添加到数据列表中
    for row_index in range(1, worksheet.nrows):  # 从第二行开始遍历，因为通常第一行是标题
        row = worksheet.row(row_index)
        item = {}
        for col_index, cell in enumerate(row):
            item[worksheet.cell_value(0, col_index)] = worksheet.cell_value(row_index, col_index)
        data.append(item)

    # 将数据写入 JSON 文件
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

    print(excel_file + " 文件已成功导出为 " + json_file + "文件！")


#打开文件
with open("ExportRules.txt", "r", encoding='utf-8') as file:
    #逐行读取文件内容
    for line in file:
        params = line.strip().split(',')
        print(params[1])
        excel_to_json(os.path.join('..', 'Datas', params[0]), os.path.join('..', 'Jsons', params[1] + '.json'))

#找到规则脚本
#读取脚本，每一行的格式：excel路径_导出名称