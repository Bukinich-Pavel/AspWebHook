using System.Text;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Examples.WebHook.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;

    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task EchoAsync(Update update)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message            => BotOnMessageReceived(update.Message!),
            UpdateType.EditedMessage      => BotOnMessageReceived(update.EditedMessage!),
            UpdateType.CallbackQuery      => BotOnCallbackQueryReceived(update.CallbackQuery!),
            UpdateType.InlineQuery        => BotOnInlineQueryReceived(update.InlineQuery!),
            UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult!),
            _                             => UnknownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation("Receive message type: {messageType}", message.Type);
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            //"/inline"   => SendInlineKeyboard(_botClient, message),
            "/Hello"      => SendReplyKeyboard(_botClient, message),
            //"/remove"   => RemoveKeyboard(_botClient, message),
            "/Photo"      => SendFile(_botClient, message),
            "/request"    => RequestContactAndLocation(_botClient, message),
            "�����������" => EducationInfo(_botClient, message),
            "������"      => WorkerInfo(_botClient, message),
            "������"      => SkillsInfo(_botClient, message),
            "<-back"      => SendReplyKeyboard(_botClient, message),
            "���"         => SkillWebInfo(_botClient, message),
            "�������"     => SkillWPFInfo(_botClient, message),
            _             => Usage(_botClient, message)
        };
        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {sentMessageId}",sentMessage.MessageId);

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
        #region SendInlineKeyboard
        static async Task<Message> SendInlineKeyboard(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            // Simulate longer running task
            await Task.Delay(500);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("�����������", "11"),
                        InlineKeyboardButton.WithCallbackData("������", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("������", "21"),
                        InlineKeyboardButton.WithCallbackData("���������", "22"),
                    },
                });

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: "Choose",
                                                  replyMarkup: inlineKeyboard);
        }
        #endregion

        static async Task<Message> SendReplyKeyboard(ITelegramBotClient bot, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "�����������", "������" },
                        new KeyboardButton[] { "������", "���������" },
                })
                {
                    ResizeKeyboard = true
                };

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: "������",
                                                  replyMarkup: replyKeyboardMarkup);
        }

        #region RemoveKeyboard
        static async Task<Message> RemoveKeyboard(ITelegramBotClient bot, Message message)
        {
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: "Removing keyboard",
                                                  replyMarkup: new ReplyKeyboardRemove());
        }
        #endregion

        static async Task<Message> SendFile(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            const string filePath = @"Files/tux.png";
            using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            return await bot.SendPhotoAsync(chatId: message.Chat.Id,
                                            photo: new InputOnlineFile(fileStream, fileName),
                                            caption: "Nice Picture");
        }

        static async Task<Message> RequestContactAndLocation(ITelegramBotClient bot, Message message)
        {
            ReplyKeyboardMarkup RequestReplyKeyboard = new(
                new[]
                {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: "Who or Where are you?",
                                                  replyMarkup: RequestReplyKeyboard);
        }

        static async Task<Message> EducationInfo(ITelegramBotClient bot, Message message)
        {
            StringBuilder str = new StringBuilder();
            str.Append("� ����� ������ ����� �������� ����������, ");
            str.Append("���������� � �������� � ����������, ");
            str.Append("�� � ������� ���� ���� �� �����).\n");
            str.Append("�������� � �������� ���(�������� ���..). ");
            str.Append("������ �� ���������������� ����������. ");
            str.Append("�� ������� ����� ����� ������� �� ��� ����� �������������, ");
            str.Append("������ � �������-����������-����������� � ���������-��������)");

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: str.ToString());
        }

        static async Task<Message> WorkerInfo(ITelegramBotClient bot, Message message)
        {
            StringBuilder str = new StringBuilder();
            str.Append("������������ � ����� �� ����������� \"�������� ��������\" ");
            str.Append("� ��������� �������-����������. ");
            str.Append("�� ����� ��������� ������ ������� �����������:\n");
            str.Append("��������� �������������,\n");
            str.Append("�������� ����� ���������,\n");
            str.Append("������ ��������(������� �� ����� ������ ����;-)),\n");
            str.Append("� ������ ������ �������� ��������� �������. ");
            str.Append("������ �� ������ ������� ���� ����� ������������� ");
            str.Append("����� ��� ����� �� ���. \n");
            str.Append("� ��� �� �������� � ���������� ������������ ");
            str.Append("�� ������ ������.(����. �����. ��������.)");

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: str.ToString());
        }

        static async Task<Message> SkillsInfo(ITelegramBotClient bot, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "<-back" },
                        new KeyboardButton[] { "���", "�������" },
                })
            {
                ResizeKeyboard = true
            };

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: "������",
                                                  replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> SkillWebInfo(ITelegramBotClient bot, Message message)
        {
            StringBuilder str = new StringBuilder();
            str.Append("���������� ���������� ��� ����. � ������ ����� ����� �� ��� �������, ");
            str.Append("����� ��� ����� ���������� � ���� ������, �� ����� �� �������\n");
            str.Append("https://bukinichweb.azurewebsites.net\n");
            str.Append("��� ������ ��������� ����� ����� ������� ��� ��� �� ����� �� ����������) ");
            str.Append("�� ��� ����������� ��������� ����� ����������� ������.");

            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "<-back" },
                        new KeyboardButton[] { "���", "�������" },
                })
            {
                ResizeKeyboard = true
            };

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: str.ToString(),
                                                  replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> SkillWPFInfo(ITelegramBotClient bot, Message message)
        {
            StringBuilder str = new StringBuilder();
            str.Append("����� :-) ");

            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "<-back" },
                        new KeyboardButton[] { "���", "�������" },
                })
            {
                ResizeKeyboard = true
            };

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: str.ToString(),
                                                  replyMarkup: replyKeyboardMarkup);
        }


        static async Task<Message> Usage(ITelegramBotClient bot, Message message)
        {
            const string usage = "���������:\n" +
                                 //"/inline   - send inline keyboard\n" +
                                 "/Hello - ���������� � ����\n" +
                                 //"/remove   - remove custom keyboard\n" +
                                 "/Photo    - ��������� ����\n" +
                                 "/request  - ������� � �������\n";

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: usage,
                                                  replyMarkup: new ReplyKeyboardRemove());
        }
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
    {
        await _botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}");

        await _botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: $"Received {callbackQuery.Data}");
    }

    #region Inline Mode

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
    {
        _logger.LogInformation("Received inline query from: {inlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "3",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent(
                    "hello"
                )
            )
        };

        await _botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                                results: results,
                                                isPersonal: true,
                                                cacheTime: 0);
    }

    private Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
    {
        _logger.LogInformation("Received inline result: {chosenInlineResultId}", chosenInlineResult.ResultId);
        return Task.CompletedTask;
    }

    #endregion

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {updateType}", update.Type);
        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(Exception exception)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }
}
