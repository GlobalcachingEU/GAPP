var map;
var gcMarker;
var curposMarker;
var wptmarkers = [];
var custonOverlays = [];
var activeInfoWindow = null;

var blueIcon = new google.maps.MarkerImage("http://www.google.com/intl/en_us/mapfiles/ms/micons/blue-dot.png");
//icons

function init() {
	/*
    Build list of map types.
    You can also use var mapTypeIds = ["roadmap", "satellite", "hybrid", "terrain", "OSM"]
    but static lists sucks when google updates the default list of map types.
    */
    var mapTypeIds = [];
    for(var type in google.maps.MapTypeId) {
        mapTypeIds.push(google.maps.MapTypeId[type]);
    }
    mapTypeIds.push("OSM");
    mapTypeIds.push("Offline");

    var myOptions = {
        zoom: 13,
		scaleControl: true,
        center: new google.maps.LatLng(0.0, 0.0),
        mapTypeId: google.maps.MapTypeId.ROADMAP,
		mapTypeControlOptions: { mapTypeIds: mapTypeIds }
    };
    map = new google.maps.Map(document.getElementById("map"), myOptions);

	//Define OSM map type pointing at the OpenStreetMap tile server
    map.mapTypes.set("OSM", new google.maps.ImageMapType({
        getTileUrl: function(coord, zoom) {
            return "http://tile.openstreetmap.org/" + zoom + "/" + coord.x + "/" + coord.y + ".png";
        },
        tileSize: new google.maps.Size(256, 256),
        name: "OpenStreetMap",
        maxZoom: 18
    }));

    map.mapTypes.set("Offline", new google.maps.ImageMapType({
        getTileUrl: function(coord, zoom) {
            return "http://localhost:6231/" + zoom + "/" + coord.x + "/" + coord.y + ".png";
        },
        tileSize: new google.maps.Size(256, 256),
        name: "Offline",
        maxZoom: 18
    }));

    map.enableKeyDragZoom({
        visualEnabled: false,
        visualPosition: google.maps.ControlPosition.LEFT,
        visualPositionOffset: new google.maps.Size(35, 0),
        visualPositionIndex: null,
        visualSprite: "http://maps.gstatic.com/mapfiles/ftr/controls/dragzoom_btn.png",
        visualSize: new google.maps.Size(20, 20),
        visualTips: {
            off: "Turn on",
            on: "Turn off"
        }
    });

    gcMarker = new google.maps.Marker({ title: 'none', 'map': map, draggable: false, icon: blueIcon, position: map.getCenter(), flat: false, visible: true });
    curposMarker = new google.maps.Marker({ title: 'none', 'map': map, draggable: false, icon: curposIcon, position: map.getCenter(), flat: true, visible: false });

	bound.pageReady();
}

function setCurrentPosition(valid, lat, lon) {
	if (valid) {
		curposMarker.setPosition(new google.maps.LatLng(lat, lon));
		curposMarker.setVisible(true);
	}
	else {
		curposMarker.setVisible(false);
	}
}

function setMapCenter(lat, lon) {
		map.setCenter(new google.maps.LatLng(lat, lon));
}

function setGeocache(cacheType, gccode, name, lat, lon, gcic) {
    if (gccode.toString().length > 0) {
        $("#title").html('<img src="' + cacheType + '" />' + gccode + ': ' + name);
		gcMarker.setPosition(new google.maps.LatLng(lat, lon));
		gcMarker.setVisible(true);
		gcMarker.setTitle(gccode);
		gcMarker.setIcon(eval(gcic));
		map.setCenter(new google.maps.LatLng(lat, lon));
    }
    else {
        $("#title").html('');
		gcMarker.setVisible(false);
    }
}

function updateWaypoints(wpList) {	
	for (var i = 0; i < wptmarkers.length; i++) {
		wptmarkers[i].setMap(null);
	}
	wptmarkers.length = 0;
	var wps = wpList;
	for (var i = 0; i < wps.length; i++) {
		wptmarkers.push(createWaypoint(wps[i].a,new google.maps.LatLng(wps[i].b, wps[i].c), eval(wps[i].d), wps[i].e));
	}
}

function createWaypoint(id,point,ic,balloonCnt) {
   var marker = new google.maps.Marker({ 'title': id, map: map, draggable: false, icon: ic, position: point, flat: false, visible: true });
   var iw = new google.maps.InfoWindow();
   iw.setContent(balloonCnt);
   google.maps.event.addListener(marker, 'click', function () {
   if (activeInfoWindow!=null) activeInfoWindow.close();
   activeInfoWindow = iw;
   iw.open(map, marker);
   });
   return marker;
}

function addPolygons(polys) {
    var ps = polys;
    for (var i=0; i<ps.length; i++) {
        createArea(ps[i]);
    }
}

function zoomToBounds(minlat, minlon, maxlat, maxlon) {
    map.fitBounds(new google.maps.LatLngBounds(new google.maps.LatLng(minlat, minlon), new google.maps.LatLng(maxlat, maxlon)));
}

function createArea(area) {
    var points = [];
    for (var i = 0; i < area.points.length; i++) {
        points.push(new google.maps.LatLng(area.points[i].lat, area.points[i].lon));
    }
    custonOverlays.push(new google.maps.Polygon({ clickable: false, paths: points, map: map, fillOpacity: 0.1 }));
}

function setCenter(lat, lon) {
    map.setCenter(new google.maps.LatLng(lat, lon));
}

function onResize() {
  $('#map').width($(document).width()-40);
  $('#map').height($(document).height()-80);
}

$(window).resize(function() {
	onResize();
});

$(document).ready(function() {
    init();
	onResize();
});