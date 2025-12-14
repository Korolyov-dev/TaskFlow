public enum ActivityType
{
    // Задачи
    TaskCreated = 1,
    TaskUpdated = 2,
    TaskDeleted = 3,
    TaskMoved = 4,
    TaskCompleted = 5,

    // Комментарии
    CommentAdded = 10,
    CommentUpdated = 11,
    CommentDeleted = 12,

    // Участники
    MemberAdded = 20,
    MemberRemoved = 21,
    MemberRoleChanged = 22,

    // Доски
    BoardCreated = 30,
    BoardUpdated = 31,
    BoardDeleted = 32,

    // Вложения
    AttachmentAdded = 40,
    AttachmentDeleted = 41,

    // Метки
    LabelAdded = 50,
    LabelRemoved = 51,
    LabelUpdated = 52,

    // Системные
    SystemNotification = 100
}
