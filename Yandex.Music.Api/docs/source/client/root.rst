YandexMusicClient
==================================================================

Класс клиента для работы с API Яндекс.Музыки. Реализует основной функционал получения
объектов для взаимодействия с API. Для самих объектов функционал реализован в виде
методов-расширений.

------------------------------------------------------------------
Свойства
------------------------------------------------------------------

**Context**
   Account, с которым работает клиент.

   **Type**: YAccount

**IsAuthorized**
   Флаг авторизации

   **Type**: bool

------------------------------------------------------------------
Методы
------------------------------------------------------------------

.. code-block:: csharp

   public YandexMusicClient(DebugSettings settings = null)

Конструктор.

.. code-block:: csharp

   public bool Authorize(string login, string password)

Авторизация с использованием логина и пароля.

.. code-block:: csharp

   public bool Authorize(string token)

Авторизация с использованием токена.

.. code-block:: csharp

   public YTrack GetTrack(string id)

Получение трека по идентификатору.

.. code-block:: csharp

   public List<YTrack> GetTracks(IEnumerable<string> ids)

Получение списка треков по идентификаторам.

.. code-block:: csharp

   public YAlbum GetAlbum(string id)

Получение альбома по идентификатору.

.. code-block:: csharp

   public List<YAlbum> GetAlbums(IEnumerable<string> ids)

Получение списка альбомов по идентификаторам.

.. code-block:: csharp

   public YLanding GetLanding(params YLandingBlockType[] blocks)

Получение блоков главной страницы.   

.. code-block:: csharp

   public YFeed Feed()

Получение ленты.   

.. code-block:: csharp

   public YArtistBriefInfo GetArtist(string id)

Получение исполнителя по идентификатору.

.. code-block:: csharp

   public List<YArtist> GetArtists(IEnumerable<string> ids)

Получение списка исполнителей по идентификаторам.

.. code-block:: csharp

   public YPlaylist GetPlaylist(string user, string id)

Получение плейлиста по пользователю и идентификатору.

.. code-block:: csharp

   public List<YPlaylist> GetPlaylists(IEnumerable<(string user, string id)> ids)

Получение списка плейлистов по пользователю и идентификатору.

.. code-block:: csharp

   public List<YPlaylist> GetPersonalPlaylists()

Получение персональных плейлистов.

.. code-block:: csharp

   public List<YPlaylist> GetFavorites()

Получение списка избранных плейлистов.

.. code-block:: csharp

   public YPlaylist GetAlice()

Получение плейлиста Алисы.

.. code-block:: csharp

   public YPlaylist GetDejaVu()

Получение плейлиста Дежавю.

.. code-block:: csharp

   public YPlaylist GetMissed()

Получение плейлиста Тайник.

.. code-block:: csharp

   public YPlaylist GetOfTheDay()

Получение плейлиста дня.

.. code-block:: csharp

   public YPlaylist GetPodcasts()

Получение плейлиста Подкасты.

.. code-block:: csharp

   public YPlaylist GetKinopoisk()

Получение плейлиста Кинопоиск.

.. code-block:: csharp

   public YPlaylist GetPremiere()

Получение плейлиста Премьера.

.. code-block:: csharp

   public YPlaylist CreatePlaylist(string name)

Создание плейлиста.

.. code-block:: csharp

   public YSearch Search(string searchText, YSearchType searchType, int page = 0)

Поиск.

.. code-block:: csharp

   public YSearchSuggest GetSearchSuggestions(string searchText)

Подсказки по поиску.

.. code-block:: csharp

   public List<YTrack> GetLikedTracks()

Получение списка понравившихся треков.

.. code-block:: csharp

   public List<YTrack> GetDislikedTracks()

Получение списка непонравившихся треков.

.. code-block:: csharp

   public List<YAlbum> GetLikedAlbums()

Получение списка понравившихся альбомов.

.. code-block:: csharp

   public List<YArtist> GetLikedArtists()

Получение списка понравившихся исполнителей.

.. code-block:: csharp

   public List<YArtist> GetDislikedArtists()

Получение списка непонравившихся исполнителей.

.. code-block:: csharp

   public List<YPlaylist> GetLikedPlaylists()

Получение списка понравившихся плейлистов.

.. code-block:: csharp

   public List<YStation> GetRadioDashboard()

Получение списка рекомендованных радиостанций.

.. code-block:: csharp

   public List<YStation> GetRadioStations()

Получение списка радиостанций.

.. code-block:: csharp

   public YStation GetRadioStation(YStationId id)

Получение радиостанции по идентификатору.