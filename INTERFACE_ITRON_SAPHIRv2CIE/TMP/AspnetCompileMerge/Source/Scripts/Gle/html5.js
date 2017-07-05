  function myCreate()
{
      var e = "abbr,article,aside,audio,bb,canvas,datagrid,datalist,details,dialog,eventsource,figure,footer,header,hgroup,mark,menu,meter,nav,output,progress,section,time,video,date".split(',');
      for (var i = 0; i < e.length; i++) { document.createElement(e[i]) }
      alert("ok");
  }


  $(document).ready(function () {
      $('.dropdown-toggle').dropdown();
  });

