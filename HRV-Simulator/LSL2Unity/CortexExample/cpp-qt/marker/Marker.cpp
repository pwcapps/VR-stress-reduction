/***************
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
***************/
#include "Marker.h"
#include <QtDebug>
#include <QCoreApplication>
#include <QDateTime>


Marker::Marker(QObject *parent) : QObject(parent) {
    connect(&client, &CortexClient::connected, this, &Marker::onConnected);
    connect(&client, &CortexClient::disconnected, this, &Marker::onDisconnected);
    connect(&client, &CortexClient::errorReceived, this, &Marker::onErrorReceived);
    connect(&client, &CortexClient::closeSessionOk, this, &Marker::onCloseSessionOK);
    connect(&client, &CortexClient::injectMarkerOk, this, &Marker::onInjectMarkerOK);

    connect(&finder, &HeadsetFinder::headsetsFound, this, &Marker::onHeadsetsFound);
    connect(&creator, &SessionCreator::sessionCreated, this, &Marker::onSessionCreated);
}

void Marker::start(QString license) {
    this->license = license;
    client.open();
}

void Marker::onConnected() {
    qInfo() << "Connected to Cortex";
    finder.findHeadsets(&client);
}

void Marker::onDisconnected() {
    qInfo() << "Disconnected";
    QCoreApplication::quit();
}

void Marker::onErrorReceived(QString method, int code, QString error) {
    qCritical() << "Cortex returned an error:";
    qCritical() << "\t" << method << code << error;
    QCoreApplication::quit();
}

void Marker::onHeadsetsFound(const QList<Headset> &headsets) {
    headsetId = headsets.first().id;
    finder.clear();
    creator.createSession(&client, headsetId, license);
}

void Marker::onSessionCreated(QString token, QString sessionId) {
    this->token = token;
    this->sessionId = sessionId;

    // after a few seconds, inject some markers
    QTimer::singleShot(5*1000, this, &Marker::injectMarker1);
    QTimer::singleShot(13*1000, this, &Marker::injectMarker2);
    QTimer::singleShot(21*1000, this, &Marker::injectStopMarker2);

    // close the session after 30 seconds
    QTimer::singleShot(30*1000, this, &Marker::closeSession);
}

void Marker::injectMarker1() {
    qInfo() << "Inject marker test1";
    client.injectMarker(token, sessionId,
                        "test1", 41,
                        QDateTime::currentMSecsSinceEpoch());
}

void Marker::injectMarker2() {
    qInfo() << "Inject marker test2";
    client.injectMarker(token, sessionId,
                        "test2", 42,
                        QDateTime::currentMSecsSinceEpoch());
}

void Marker::injectStopMarker2() {
    qInfo() << "Inject stop marker for test2";
    client.injectStopMarker(token, sessionId,
                        "test2", 42,
                        QDateTime::currentMSecsSinceEpoch());
}

void Marker::onInjectMarkerOK() {
    qInfo() << "Inject marker OK";
}

void Marker::closeSession() {
    qInfo() << "Closing the session";
    client.closeSession(token, sessionId);
}

void Marker::onCloseSessionOK() {
    client.close();
}
