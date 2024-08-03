from transformers import MarianMTModel, MarianTokenizer
import xlrd
import xlwt
import os

def translate_text(text, tokenizer, model):
    # 预处理输入文本
    inputs = tokenizer(text, return_tensors='pt', padding=True, truncation=True)
    # 生成翻译
    translated = model.generate(**inputs)
    # 解码翻译结果
    translated_text = tokenizer.batch_decode(translated, skip_special_tokens=True)[0]
    return translated_text

def translate_excel(excel_file, dest_excel_file, model_name):
    # 加载模型和 tokenizer
    # 通过环境变量获取 Hugging Face API 令牌
    token = os.getenv('HUGGINGFACE_TOKEN')
    tokenizer = MarianTokenizer.from_pretrained(model_name, token=token)
    model = MarianMTModel.from_pretrained(model_name, token=token)
    
    # 打开 Excel 文件
    workbook = xlrd.open_workbook(excel_file)
    worksheet = workbook.sheet_by_index(0)

    # 创建新的 Excel 文件
    wb = xlwt.Workbook()
    ws = wb.add_sheet('Sheet1')
    ws.write(0, 0, 'ID')
    ws.write(0, 1, 'Str')

    total_rows = worksheet.nrows - 1  # 总行数，减去标题行
    for row_index in range(1, worksheet.nrows):  # 从第二行开始遍历
        try:
            print(f"Processing {row_index}/{total_rows} ({(row_index / total_rows) * 100:.2f}%)", end='\r')
            original_text = worksheet.cell_value(row_index, 1)
            translated_text = translate_text(original_text, tokenizer, model)
            ws.write(row_index, 0, worksheet.cell_value(row_index, 0))
            ws.write(row_index, 1, translated_text)
        except Exception as e:
            print(f"Error processing row {row_index}: {e}")
            ws.write(row_index, 0, worksheet.cell_value(row_index, 0))
            ws.write(row_index, 1, "Error")

    # 保存新的 Excel 文件
    wb.save(dest_excel_file)
    print(f"Translation complete. File saved as {dest_excel_file}")

# 读取 TranslationRules.txt 文件并执行翻译任务
def main():
    with open("TranslationRules.txt", "r", encoding='utf-8') as file:
        for line in file:
            params = line.strip().split(',')
            excel_file = os.path.join('..', 'Datas', params[0])
            dest_excel_file = os.path.join('..', 'Datas',params[1])
            model_name = params[2]
            translate_excel(excel_file, dest_excel_file, model_name)

if __name__ == "__main__":
    main()
