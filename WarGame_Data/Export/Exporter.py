import xlrd
import json
import os

def parse_object_string(object_str):
    # 去除大括号并根据逗号分割字符串
    parts = object_str.strip('{}').split(',')
    
    # 初始化一个空字典
    obj_dict = {}
    
    # 遍历分割后的字符串，解析每个键值对并添加到字典中
    for part in parts:
        # 根据冒号分割键值对
        key, value = part.split(':')
        # 去除空格并添加到字典中
        obj_dict[key.strip()] = int(value.strip()) if value.strip().isdigit() else value.strip()

    return obj_dict

def parse_cell_value(cell):
    # 根据单元格的类型解析值
    if cell.ctype == xlrd.XL_CELL_NUMBER:
        # 如果是数字类型，根据是否为整数判断是整型还是浮点型
        if cell.value.is_integer():
            return int(cell.value)
        else:
            return float(cell.value)
    elif cell.ctype == xlrd.XL_CELL_TEXT:
        # 如果是文本类型，尝试解析为对象字符串，否则直接返回文本
        try:
            return parse_object_string(cell.value)
        except ValueError:
            return cell.value.strip()
    elif cell.ctype == xlrd.XL_CELL_EMPTY:
        # 如果是空单元格，返回 None
        return None
    elif cell.ctype == xlrd.XL_CELL_BOOLEAN:
        # 如果是布尔类型，返回对应的布尔值
        return bool(cell.value)
    elif cell.ctype == xlrd.XL_CELL_DATE:
        # 如果是日期类型，返回日期值
        return xlrd.xldate.xldate_as_datetime(cell.value, 0)
    else:
        # 其他类型暂时统一返回字符串
        return str(cell.value)

def excel_to_json(excel_file, json_file):
    # 打开 Excel 文件
    workbook = xlrd.open_workbook(excel_file)
    # 获取第一个工作表
    worksheet = workbook.sheet_by_index(0)

    # 初始化一个空的 JSON 数据列表
    data = []

    # 遍历工作表的每一行，将数据解析为对象，并添加到数据列表中
    for row_index in range(1, worksheet.nrows):  # 从第二行开始遍历，因为通常第一行是标题
        row = worksheet.row(row_index)
        item = {}
        for col_index, cell in enumerate(row):
            # 将单元格的值根据类型解析为对应的数据类型
            item[worksheet.cell_value(0, col_index)] = parse_cell_value(cell)
        data.append(item)

    # 将数据写入 JSON 文件
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

    print(excel_file + " 文件已成功导出为 " + json_file + " 文件！")

# 打开文件
with open("ExportRules.txt", "r", encoding='utf-8') as file:
    # 逐行读取文件内容
    for line in file:
        params = line.strip().split(',')
        print(params[1])
        excel_to_json(os.path.join('..', 'Datas', params[0]), os.path.join('..', 'Jsons', params[1] + '.json'))
