from transformers import MarianMTModel, MarianTokenizer
import xlrd
import xlwt
import os

# 加载模型和 tokenizer
# 通过环境变量获取 Hugging Face API 令牌
token = os.getenv('HUGGINGFACE_TOKEN')
model_name = 'Helsinki-NLP/opus-mt-en-jap'
tokenizer = MarianTokenizer.from_pretrained(model_name, token=token)
model = MarianMTModel.from_pretrained(model_name, token=token)
    
def translate_text(text, tokenizer, model):
    # 预处理输入文本
    inputs = tokenizer(text, return_tensors='pt', padding=True, truncation=True)
    # 生成翻译
    translated = model.generate(**inputs)
    # 解码翻译结果
    translated_text = tokenizer.batch_decode(translated, skip_special_tokens=True)[0]
    return translated_text

def translate_excel():
    print(translate_text('The shaded areas are known for their tall trees and thick forests, interwoven between the treehouses and the drawbridges, where the inhabitants live in trees and are integrated with nature. The air here is fresh, the smell of birds and flowers, the ideal place for meditation and relief.The shaded areas are known for their tall trees and thick forests, interwoven between the treehouses and the drawbridges, where the inhabitants live in trees and are integrated with nature. The air here is fresh, the smell of birds and flowers, the ideal place for meditation and relief.', tokenizer, model))

# 读取 TranslationRules.txt 文件并执行翻译任务
def main():
    translate_excel()

if __name__ == "__main__":
    main()
