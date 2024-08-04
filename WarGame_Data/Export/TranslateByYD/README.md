# 说明
项目为有道智云paas接口的python语言调用示例。您可以通过执行项目中的main函数快速调用有道智云相关api服务。

# 运行环境
1. python 3.6版本及以上。

# 运行方式
1. 修改Translate.py文件，注册有道智云账号，获得id和密钥。
3. 新增TranslationRules.txt条目，格式为 源文件，目标文件，源语种，目标语种（语种标识符可以在网易云官网查询https://ai.youdao.com/DOCSIRMA/html/trans/api/yyfy/index.html）

# 注意事项
1. 项目中的代码有些仅作展示及参考，生产环境中请根据业务的实际情况进行修改。
2. 项目中接口返回的数据仅在控制台输出，实际使用中请根据实际情况进行解析。