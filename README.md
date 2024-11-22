1. Настроить разрешение окна 600/1000

========================================

2. Сделать верстку окна: 
	2.1 Grid.RowDefinitions 75/*/30;
	[2.2 Добавить в ресурсы "logo"]
	2.2 Вставить изображение: <Image Source ="..." HorizontalAligment="Left">
	2.3 Вставить заголовок: <TextBlock Text = "Header" HorizontalAligment = center, VerticalAligment = center, FontSize = 30>
	2.4 Установить цвета для верхней нижней части: <Grid Background = "#bae38e" Panel.ZIndex = "-2"> 
	<Grid Grid.Row="2" Background="#445c93">
	2.5 Вставить кнопку назад <Button Content = "Back" HorizontalAligment="Right" Height = "30", Width = "175">
		После 3.5 пункта добавить событие к frameMain: MainFrame_ContentRendered и обработку:
		    if (FrameMain.CanGoBack)
            {
                BtnBack.Visibility = Visibility.Visible;
            }
            else 
            { 
                BtnBack.Visibility = Visibility.Collapsed; 
            }

=========================================

3. Создание навигации
	3.1 Вставить в верстку <Frame Grid.Row = 1, Name = "FrameMain" ContentRendered="MainFrame_ContentRendered" 
	NavigationUIVisibility='Hidden'>
	3.2 Добавить две страницы (PageMain, PageAddEdit)
	3.3 В коде окна добавить в конструктор переход на страницу: FrameMain.Navigate(new PageMain());
	3.4 Добавим класс Manager и поле: public static Frame FrameMain {get;set;}
	3.5 в пункт 3.3 добавляем ниже строку: Manager.FrameMain = FrameMain;
	3.6 Обработка кнопки на главное СТРАНИЦЕ: Manager.FrameMain.Navigate(new PageAddEdit());
	
==========================================

4. Подключение/работа с БД.

В папку Entities подключить entity БД
	4.1 Открываем BaseModel.Context.cs дописываем поле: private static NobelLaureatesEntities _context;
	4.2 Добавляем метод получения экземпляра этого контекста: 
	        public static NobelLaureatesEntities GetContext() 
        { 
            if (_context == null) _context = new NobelLaureatesEntities();

            return _context;
        }
	--- MainPage Верстка ---
	        <Grid.RowDefinitions>
            <RowDefinition Height="377*"/>
            <RowDefinition Height="73*"/>
        </Grid.RowDefinitions>
		
        <DataGrid x:Name="DGridMain" AutoGenerateColumns="false" IsReadOnly="True">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Фамилия" Binding="{Binding Фамилия}"/>
                <DataGridTextColumn Header="Имя" Binding="{Binding Имя}"/>
                <DataGridTextColumn Header="Отчество" Binding="{Binding Отчество}"/>
                <DataGridTextColumn Header="Страна" Binding="{Binding Страна}"/>
                <DataGridCheckBoxColumn Header="Жив" Binding="{Binding Жив}"/>
                <DataGridTemplateColumn Width="auto">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <Button Content="Редактировать"
                                    Name="BtnEdit"
                                    Background="#fac716" 
                                    Width="175" 
                                    Height="30" 
                                    Click="Button_Click"/>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>
        <Button Content="Добавить" Grid.Row="1" 
                HorizontalAlignment="Left" 
                Name="BtnAdd" 
                Click="BtnAdd_Click"
                Margin="5"
                Background="#fac716" 
                Width="175"
                Height="30"/>
        <Button Content="Удалить" Grid.Row="1" 
                HorizontalAlignment="Right" 
                Name="BtnDelete" 
                Click="BtnDelete_Click"
                Margin="5"
                Background="#fac716" 
                Width="175"
                Height="30"/>
				
	4.3 Далее загрузим список отелей в коде в таблицу:

a) Нажимаем F7
b) Обращаемся к контексту модели: DGridMain.ItemsSource = NobelLaureatesEntities.GetContext().Laureaties.ToList();

=== Add/Edit верстка ===
[AddEdit.cs] 
private Base _currentTable = new Base();

1. В конструкторе public AddEdit() дописать:
DataContext = _currentTable;
2. В верстке TextBox добавить Text={Binding Поле}
   -Если comboBox-
   SelectedItem="{Binding Country} DisplayMemberPath = {Binding Name}"
   
3. StringBuilder errors/errors.AppendLines(" Текст ошибки ");
   if (errors.Length > 0) {MessageBox.Show(errors.ToString());
   return;}
   
4. if (_currentTable.Id == 0) BaseEntities.GetContext().Base.Add(_currentTable)
   try
   {
	BaseEntities.GetContext().SaveChanges();
	MessageBox("Save");
   } catch (Exception ex) {/*Vivod*/}

-- [+Событие в MainPage] IsVisibleChange --
.cs: if (Visibility == Visibility.Visible) 
{
	BaseEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
 	DGridBase.ItemSource = BaseEntities.GetContext().Base.ToList();
}
5. Функция редактирования:
Change struct:
        public PageAddEdit(Laureaty selected_laureat)
        {
            InitializeComponent();

            if (selected_laureat != null)
            {
                _currentLaureat = selected_laureat;
            }
            DataContext = _currentLaureat;
        }

1. в mainpage:
           private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Manager.FrameMain.Navigate(new PageAddEdit((sender as Button).DataContext as Laureaty));
        }
   
2. Delete:
   	private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var laureatyForRemoving = DGridMain.SelectedItems.Cast<Laureaty>().ToList();

            if (MessageBox.Show($"Удалить следующие: {laureatyForRemoving.Count()} элементов?", 
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) 
            {
                try
                {
                    NobelLaureatesEntities.GetContext().Laureaties.RemoveRange(laureatyForRemoving);
                    NobelLaureatesEntities.GetContext().SaveChanges();
                    MessageBox.Show("Данные удалены!");

                    DGridMain.ItemsSource = NobelLaureatesEntities.GetContext().Laureaties.ToList();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }


