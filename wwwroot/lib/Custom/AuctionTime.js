var ShowAuctionTiming = function (dateAr, sysDate) {

    //var dateAr='14-08-2017 1:00:00 PM'.split('-');
    var dateAr = dateAr.split('-');
    var end = new Date(dateAr[1] + '/' + dateAr[0] + '/' + dateAr[2]);

    var currentDate = sysDate.split(' ')[0].split('-');
    var currentTime = sysDate.split(' ')[1].split(':');
    var now = new Date(currentDate[2], currentDate[1] - 1, currentDate[0], currentTime[0], currentTime[1], currentTime[2]);

    //alert(end);
    var _second = 1000;
    var _minute = _second * 60;
    var _hour = _minute * 60;
    var _day = _hour * 24;
    var timer;

    function showRemaining() {

        now.setSeconds(now.getSeconds() + 1);

        var distance = end - now;

        if (distance < 0) {

            clearInterval(timer);
            document.getElementById('clockdiv').innerHTML = 'EXPIRED!';

            window.location.href = "GetWinners";
            return;
        }
        var days = Math.floor(distance / _day);
        var hours = Math.floor((distance % _day) / _hour);
        var minutes = Math.floor((distance % _hour) / _minute);
        var seconds = Math.floor((distance % _minute) / _second);

        document.getElementById('DaysSp').innerHTML = days;
        document.getElementById('HoursSP').innerHTML = hours;
        document.getElementById('mintSP').innerHTML = minutes;
        document.getElementById('secSP').innerHTML = seconds;
    }

    timer = setInterval(showRemaining, 1000);
}