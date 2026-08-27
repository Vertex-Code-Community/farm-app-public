using Microsoft.AspNetCore.Components;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;

namespace FarmApp.Components.Pages.Guide;

public partial class GuidePage
{
    [Inject] public INavigationService NavigationService { get; set; } = null!;
    [Parameter] public bool ShowBackButton { get; set; }
    
    private class GuideItemModel
    {
        public required string Header { get; set; }
        public required string Subtitle { get; set; }
        public required string Description { get; set; }
    }
    
    private int _index = 0;
    private GuideItemModel SelectedItem => _items[_index];

    private readonly List<GuideItemModel> _items = new List<GuideItemModel>()
    {
        new GuideItemModel
        {
            Header = "Об’єднання полів",
            Subtitle = "Як увімкнути режим",
            Description = "Щоб об’єднати ділянки в одне поле, увімкніть режим об’єднання полів."
        },
        new GuideItemModel
        {
            Header = "Об’єднання полів",
            Subtitle = "Вибір на карті",
            Description = "Натисніть на ділянки поруч для обʼєднання їх в 1 поле (якщо згодом ви видалете поле, ділянки зʼявляться на своєму місці)"
        },
        new GuideItemModel
        {
            Header = "Режим зберігання",
            Subtitle = "Підтвердження дій",
            Description = "Для збереження поля натисніть кнопку «Зберегти» праворуч."
        },
        new GuideItemModel
        {
            Header = "Назва поля",
            Subtitle = "Назва для нового поля",
            Description = "Введіть назву вашого поля та збережіть його."
        },
        new GuideItemModel
        {
            Header = "Назва поля",
            Subtitle = "Перевірка перед збереженням",
            Description = "Введіть назву вашого поля та збережіть його."
        },
        new GuideItemModel
        {
            Header = "Попередній перегляд",
            Subtitle = "Поле на карті",
            Description = "Тепер на карті ви можете бачити створене вами поле."
        },
        new GuideItemModel
        {
            Header = "Створення записів",
            Subtitle = "Календар і нотатки",
            Description = "Для створення записів для цього поля перейдіть на сторінку календаря."
        },
        new GuideItemModel
        {
            Header = "Додавання записів",
            Subtitle = "Місяць і день",
            Description = "Тут ви можете відкрити необхідний місяць та натиснути на день задля перегляду та редагування записів."
        },
        new GuideItemModel
        {
            Header = "Відображення записів",
            Subtitle = "Зв’язок із датою",
            Description = "Після успішного додавання записів, вони будуть відображені біля дати до якої вони закріплені."
        },
        new GuideItemModel
        {
            Header = "Швидкий перегляд",
            Subtitle = "З деталей поля",
            Description = "Для швидкого перегляду запису, використовуйте кнопку «Календар» біля відповідного запису у вікні перегляду деталей поля на карті."
        },
        new GuideItemModel
        {
            Header = "Перегляд ділянки",
            Subtitle = "Інформація на карті",
            Description = "Для перегляду інформації по обраній ділянці, натисніть на неї на карті."
        },
        new GuideItemModel
        {
            Header = "Редагування ділянки",
            Subtitle = "Зміна контуру",
            Description = "Натиснувши кнопку редагування, ви зможете змінювати геометрію обраної ділянки."
        },
        new GuideItemModel
        {
            Header = "Зберігання ділянки",
            Subtitle = "Зберегти зміни",
            Description = "Для збереження змін, використовуйте кнопку «Зберегти»."
        },
        new GuideItemModel
        {
            Header = "Переміщення ділянки",
            Subtitle = "Окремий режим",
            Description = "Для переміщення ділянки на карті, увімкніть режим переміщення."
        },
        new GuideItemModel
        {
            Header = "Створення ділянки",
            Subtitle = "Увімкнути режим малювання",
            Description = "Для створення нової ділянки увімкніть режим створення нової ділянки."
        },
        new GuideItemModel
        {
            Header = "Створення ділянки",
            Subtitle = "Місце і форма",
            Description = "Натисніть на карті на місце де нова ділянка має бути створена та змініть її геометрію якщо потрібно."
        },
        new GuideItemModel
        {
            Header = "Перегляд інформації",
            Subtitle = "Після збереження",
            Description = "Після збереження ділянки, ви можете переглянути інформацію щодо неї."
        }
    };

    private void OnNextClicked()
    {
        _index++;

        if (_index >= _items.Count - 1)
        {
            _index = _items.Count - 1;
            if (!ShowBackButton) GoToMainPage();
        }
    }

    private void GoToMainPage()
    {
        NavigationService.History.Clear();
        NavigationService.NavigateTo(Constants.ClientRoutes.MainPage);
    }
}