using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterDataGrid
{
   public enum Local
    {
        TraditionalChinese,//简体中文
        SimplifiedChinese,//繁体中文
        Dutch,//荷兰语
        English,//英语
        French,//法语
        German,//德语
        Italian,//意大利语
        Polish,//波兰语
        Russian,//俄语
        Spanish,//西班牙语
        Japanese,//日语
        Korean   //韩语
    }

    public class Loc
    {
        #region Private Fields

        private Local language;

        // culture name(used for dates)
        private static readonly Dictionary<Local, string> CultureNames = new Dictionary<Local, string>
        {
            { Local.TraditionalChinese, "zh-Hant" },
            { Local.SimplifiedChinese, "zh-Hans" },
            { Local.Dutch,   "nl-NL" },
            { Local.English, "en-US" },
            { Local.French,  "fr-FR" },
            { Local.German,  "de-DE" },
            { Local.Italian, "it-IT" },
            { Local.Polish,  "pl-PL" },
            { Local.Russian, "ru-RU" },
            { Local.Spanish, "es-ES" },
            { Local.Japanese, "ja-JP" },
            { Local.Korean, "ko-KO" }
        };

        /// <summary>
        /// Translation dictionary
        /// </summary>
        private static readonly Dictionary<string, Dictionary<Local, string>> Translation =
            new Dictionary<string, Dictionary<Local, string>>
            {
                {
                    "All", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "(全選)" },
                        { Local.SimplifiedChinese, "(全选)" },
                        { Local.Dutch,   "(Alles selecteren)" },
                        { Local.English, "(Select all)" },
                        { Local.French,  "(Sélectionner tout)" },
                        { Local.German,  "(Alle auswählen)" },
                        { Local.Italian, "(Seleziona tutto)" },
                        { Local.Polish, "(Zaznacz wszystkie)" },
                        { Local.Russian, "(Выбрать все)" },
                        { Local.Spanish, "(Seleccionar todos)" },
                        { Local.Japanese, "(すべて選択)" } ,
                        { Local.Korean, "(모두 선택)" },
                    }
                },
                {
                    "Empty", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "(空白)" },
                        { Local.SimplifiedChinese, "(空白)" },
                        { Local.Dutch,   "(Leeg)" },
                        { Local.English, "(Blank)" },
                        { Local.French,  "(Vides)" },
                        { Local.German,  "(Leer)" },
                        { Local.Italian, "(Vuoto)" },
                        { Local.Polish, "(Pusty)" },
                        { Local.Russian, "(Заготовки)" },
                        { Local.Spanish, "(Vacio)"},
                        { Local.Japanese, "(空白)" } ,
                        { Local.Korean, "(비어 있음)" },
                    }
                },
                {
                    "Clear", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "清除篩選" },
                        { Local.SimplifiedChinese, "清除过滤器" },
                        { Local.Dutch,   "Filter verwijderen" },
                        { Local.English, "Clear filter" },
                        { Local.French,  "Effacer le filtre" },
                        { Local.German,  "Filter löschen " },
                        { Local.Italian, "Cancella filtro " },
                        { Local.Polish, "Wyczyść filtr " },
                        { Local.Russian, "Очистить фильтр" },
                        { Local.Spanish, "Limpiar filtros" },
                        { Local.Japanese, "からフィルターをクリア" } ,
                        { Local.Korean, "필터 지우기" },
                    }
                },

                {
                    "Contains", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "搜尋(包含)" },
                        { Local.SimplifiedChinese, "搜索(包含)" },
                        { Local.Dutch,   "Zoek (bevat)" },
                        { Local.English, "Search (contains)" },
                        { Local.French,  "Rechercher (contient)" },
                        { Local.German,  "Suche (enthält)" },
                        { Local.Italian, "Cerca (contiene)" },
                        { Local.Polish, "Szukaj (zawiera)" },
                        { Local.Russian, "Искать (содержит)" },
                        { Local.Spanish, "Buscar (contiene)" },
                        { Local.Japanese, "検索 (含む)" } ,
                        { Local.Korean, "검색(포함)" },
                    }
                },

                {
                    "Ok", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "確定" },
                        { Local.SimplifiedChinese, "确定" },
                        { Local.Dutch,   "Ok" },
                        { Local.English, "Ok" },
                        { Local.French,  "Ok" },
                        { Local.German,  "Ok" },
                        { Local.Italian, "Ok" },
                        { Local.Polish,  "Ok" },
                        { Local.Russian, "Ok" },
                        { Local.Spanish, "Aceptar" },
                        { Local.Japanese, "确定" },
                        { Local.Korean, "확실한가요" },
                    }
                },

                {
                    "Cancel", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "取消" },
                        { Local.SimplifiedChinese, "取消" },
                        { Local.Dutch,   "Annuleren" },
                        { Local.English, "Cancel" },
                        { Local.French,  "Annuler" },
                        { Local.German,  "Abbrechen" },
                        { Local.Italian, "Annulla" },
                        { Local.Polish,  "Anuluj" },
                        { Local.Russian, "Отмена" },
                        { Local.Spanish, "Cancelar" },
                        { Local.Japanese, "取り消し" },
                        { Local.Korean, "취소" },
                    }
                },

                {
                    "True", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "已選中" },
                        { Local.SimplifiedChinese, "已选中" },
                        { Local.Dutch,   "Aangevinkt" },
                        { Local.English, "Checked" },
                        { Local.French,  "Coché" },
                        { Local.German,  "Ausgewählt" },
                        { Local.Italian, "Controllato" },
                        { Local.Polish,  "Zaznaczone" },
                        { Local.Russian, "Проверено" },
                        { Local.Spanish, "Comprobado" },
                        { Local.Japanese, "選択済み" },
                        { Local.Korean, "선택한" },
                    }
                },

                {
                    "False", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "未選中" },
                        { Local.SimplifiedChinese, "未选中" },
                        { Local.Dutch,   "Niet aangevinkt" },
                        { Local.English, "Unchecked" },
                        { Local.French,  "Décoché" },
                        { Local.German,  "Nicht ausgewählt" },
                        { Local.Italian, "Deselezionato" },
                        { Local.Polish,  "Niezaznaczone" },
                        { Local.Russian, "непроверено" },
                        { Local.Spanish, "Sin marcar" },
                        { Local.Japanese, "未選択" },
                         { Local.Korean, "선택" },
                    }
                },

                {
                    "RemoveAll", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "清除所有篩選" },
                        { Local.SimplifiedChinese, "删除所有过滤器" },
                        { Local.Dutch,   "Alle filters verwijderen" },
                        { Local.English, "Remove all filters" },
                        { Local.French,  "Supprimer tous les filtres" },
                        { Local.German,  "Alle Filter entfernen" },
                        { Local.Italian, "Rimuovi tutti i filtri" },
                        { Local.Polish,  "Usuń wszystkie filtry" },
                        { Local.Russian, "Удалить все фильтры" },
                        { Local.Spanish, "Eliminar todos los filtros" },
                        { Local.Japanese, "すべてのフィルターをクリア" },
                        { Local.Korean, "모든 필터 삭제" },
                    }
                },

                {
                    "AscSort", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "升序排序" },
                        { Local.SimplifiedChinese, "升序排序" },
                        { Local.Dutch,   "Oplopende volgorde" },
                        { Local.English, "Ascending order" },
                        { Local.French,  "Ordre croissant" },
                        { Local.German,  "Aufsteigende Reihenfolge" },
                        { Local.Italian, "Ordine crescente" },
                        { Local.Polish,  "Porządek rosnący" },
                        { Local.Russian, "порядок по возрастанию" },
                        { Local.Spanish, "Orden ascendente" },
                        { Local.Japanese, "昇順" },
                        { Local.Korean, "오름차순" },
                    }
                },

                {
                    "DescSort", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "降序排序" },
                        { Local.SimplifiedChinese, "降序排序" },
                        { Local.Dutch,   "Aflopende" },
                        { Local.English, "Descending order" },
                        { Local.French,  "Ordre décroissant" },
                        { Local.German,  "Absteigende Reihenfolge" },
                        { Local.Italian, "Decrescente" },
                        { Local.Polish,  "Malejącym" },
                        { Local.Russian, "Порядок убывания" },
                        { Local.Spanish, "Orden descendente" },
                        { Local.Japanese, "降順" },
                        { Local.Korean, "내림차순" },
                    }
                },

                {
                    "ClearSort", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "清除排序" },
                        { Local.SimplifiedChinese, "清除排序" },
                        { Local.Dutch,   "Duidelijke sortering" },
                        { Local.English, "Clear sorting" },
                        { Local.French,  "Tri clair" },
                        { Local.German,  "Sortierung löschen" },
                        { Local.Italian, "Ordinamento chiaro" },
                        { Local.Polish,  "Wyczyść sortowanie" },
                        { Local.Russian, "Очистка сортировки" },
                        { Local.Spanish, "Clasificación clara" },
                        { Local.Japanese, "明確な並べ替え" },
                        { Local.Korean, "정렬지우기" },
                    }
                },

                {
                    "ClearAllSort", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "清除所有排序" },
                        { Local.SimplifiedChinese, "清除所有排序" },
                        { Local.Dutch,   "Wis alle soorten" },
                        { Local.English, "Clear all sorts" },
                        { Local.French,  "Effacer toutes sortes de choses" },
                        { Local.German,  "Alle Arten löschen" },
                        { Local.Italian, "Cancella tutti i tipi" },
                        { Local.Polish,  "Wyczyść wszystkie rodzaje" },
                        { Local.Russian, "Очистить все виды" },
                        { Local.Spanish, "Despeja todo tipo" },
                        { Local.Japanese, "すべての種類をクリアする" },
                        { Local.Korean, "모든 정렬 지우기" },
                    }
                },

                {
                    "CustomFilter", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "自訂篩選" },
                        { Local.SimplifiedChinese, "自定义筛选" },
                        { Local.Dutch,   "Aangepaste filtering" },
                        { Local.English, "Custom filtering" },
                        { Local.French,  "Filtrage personnalisé" },
                        { Local.German,  "Benutzerdefinierte Filterung" },
                        { Local.Italian, "Filtro personalizzato" },
                        { Local.Polish,  "Filtrowanie niestandardowe" },
                        { Local.Russian, "Пользовательская фильтрация" },
                        { Local.Spanish, "Filtrado personalizado" },
                        { Local.Japanese, "カスタムフィルタリング" },
                        { Local.Korean, "사용자 지정 필터링" },
                    }
                },

                {
                    "ClearCurrentFilter", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "清除目前篩選" },
                        { Local.SimplifiedChinese, "清除当前筛选" },
                        { Local.Dutch,   "Hiermee wist u het huidige filter" },
                        { Local.English, "Clears the current filter" },
                        { Local.French,  "Efface le filtre actuel" },
                        { Local.German,  "Löscht den aktuellen Filter" },
                        { Local.Italian, "Cancella il filtro corrente" },
                        { Local.Polish,  "Czyści bieżący filtr" },
                        { Local.Russian, "Очищает текущий фильтр" },
                        { Local.Spanish, "Borra el filtro actual" },
                        { Local.Japanese, "現在のフィルターをクリアします" },
                        { Local.Korean, "현재 필터를 지웁니다" },
                    }
                },

                {
                    "ClearAllFilter", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "清除所有篩選" },
                        { Local.SimplifiedChinese, "清除所有筛选" },
                        { Local.Dutch,   "Alle filters wissen" },
                        { Local.English, "Clear all filters" },
                        { Local.French,  "Effacer tous les filtres" },
                        { Local.German,  "Alle Filter löschen" },
                        { Local.Italian, "Cancella tutti i filtri" },
                        { Local.Polish,  "Wyczyść wszystkie filtry" },
                        { Local.Russian, "Очистить все фильтры" },
                        { Local.Spanish, "Borrar todos los filtros" },
                        { Local.Japanese, "すべてのフィルターをクリアする" },
                        { Local.Korean, "필터 모두 해제" },
                    }
                },

                {
                    "HideColumn", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "隱藏列" },
                        { Local.SimplifiedChinese, "隐藏列" },
                        { Local.Dutch,   "Kolommen verbergen" },
                        { Local.English, "Hide columns" },
                        { Local.French,  "Masquer les colonnes" },
                        { Local.German,  "Spalten ausblenden" },
                        { Local.Italian, "Nascondere le colonne" },
                        { Local.Polish,  "Ukryj kolumny" },
                        { Local.Russian, "Скрыть столбцы" },
                        { Local.Spanish, "Ocultar columnas" },
                        { Local.Japanese, "列を非表示にする" },
                        { Local.Korean, "열 숨기기" },
                    }
                },

                {
                    "SelectColumn", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "列選擇" },
                        { Local.SimplifiedChinese, "列选择" },
                        { Local.Dutch,   "Kolom selectie" },
                        { Local.English, "Column selection" },
                        { Local.French,  "Sélection des colonnes" },
                        { Local.German,  "Spaltenauswahl" },
                        { Local.Italian, "Selezione delle colonne" },
                        { Local.Polish,  "Wybór kolumn" },
                        { Local.Russian, "Выбор столбца" },
                        { Local.Spanish, "Selección de columna" },
                        { Local.Japanese, "列の選択" },
                        { Local.Korean, "열 선택" },
                    }
                },

                {
                    "SearchWord", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "請輸入需要搜索的關鍵字" },
                        { Local.SimplifiedChinese, "请输入需要搜索的关键字" },
                        { Local.Dutch,   "Voer het trefwoord in waarnaar u wilt zoeken" },
                        { Local.English, "Please enter the keyword you want to search" },
                        { Local.French,  "Veuillez saisir le mot-clé que vous souhaitez rechercher" },
                        { Local.German,  "Bitte geben Sie das Stichwort ein, nach dem Sie suchen möchten" },
                        { Local.Italian, "Inserisci la parola chiave che desideri cercare" },
                        { Local.Polish,  "Wpisz słowo kluczowe, które chcesz wyszukać" },
                        { Local.Russian, "Пожалуйста, введите ключевое слово, которое вы хотите найти" },
                        { Local.Spanish, "Introduce la palabra clave que quieres buscar" },
                        { Local.Japanese, "検索したいキーワードを入力してください" },
                        { Local.Korean, "검색하려는 키워드를 입력하십시오" },
                    }
                },

                {
                    "SearchPanel", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "搜尋面板" },
                        { Local.SimplifiedChinese, "搜索面板" },
                        { Local.Dutch,   "Deelvenster Zoeken" },
                        { Local.English, "Search panel" },
                        { Local.French,  "Panneau de recherche" },
                        { Local.German,  "Bedienfeld \"Suchen\"" },
                        { Local.Italian, "Pannello di ricerca" },
                        { Local.Polish,  "Panel wyszukiwania" },
                        { Local.Russian, "Панель поиска" },
                        { Local.Spanish, "Panel de búsqueda" },
                        { Local.Japanese, "検索パネル" },
                        { Local.Korean, "검색 패널" },
                    }
                },

                {
                    "CloseSearchPanel", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "關閉搜尋面板" },
                        { Local.SimplifiedChinese, "关闭搜索面板" },
                        { Local.Dutch,   "Sluit het zoekvenster" },
                        { Local.English, "Close the search panel" },
                        { Local.French,  "Fermer le panneau de recherche" },
                        { Local.German,  "Schließen des Suchfensters" },
                        { Local.Italian, "Chiudi il pannello di ricerca" },
                        { Local.Polish,  "Zamykanie panelu wyszukiwania" },
                        { Local.Russian, "Закройте панель поиска" },
                        { Local.Spanish, "Cerrar el panel de búsqueda" },
                        { Local.Japanese, "検索パネルを閉じる" },
                        { Local.Korean, "검색 패널 닫기" },
                    }
                }

            };

        #endregion Private Fields

        #region Constructors

        public Loc()
        {
            Language = Local.English;
        }

        #endregion Constructors

        #region Public Properties

        public Local Language
        {
            get => language;
            set
            {
                language = value;
                Culture = new CultureInfo(CultureNames[value]);
            }
        }

        public CultureInfo Culture { get; private set; }

        public string CultureName => CultureNames[Language];

        public string LanguageName => Enum.GetName(typeof(Local), Language);

        public string All => Translate("All");
        public string Cancel => Translate("Cancel");
        public string Clear => Translate("Clear");
        public string Contains => Translate("Contains");
        public string Empty => Translate("Empty");
        public string Ok => Translate("Ok");
        public string RemoveAll => Translate("RemoveAll");
        public string IsTrue => Translate("True");
        public string IsFalse => Translate("False");
        public string AscSort => Translate("AscSort");
        public string DescSort => Translate("DescSort");
        public string ClearSort => Translate("ClearSort");
        public string ClearAllSort => Translate("ClearAllSort");
        public string CustomFilter => Translate("CustomFilter");
        public string ClearCurrentFilter => Translate("ClearCurrentFilter");
        public string ClearAllFilter => Translate("ClearAllFilter");
        public string HideColumn => Translate("HideColumn");
        public string SelectColumn => Translate("SelectColumn");
        public string SearchWord => Translate("SearchWord");
        public string SearchPanel => Translate("SearchPanel");
        public string CloseSearchPanel => Translate("CloseSearchPanel");
        






        public string Neutral => "{0}";

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        /// Translated into the language
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string Translate(string key)
        {
            return Translation.ContainsKey(key) && Translation[key].ContainsKey(Language)
                ? Translation[key][Language]
                : "unknow";
        }

        #endregion Private Methods
    }
}
