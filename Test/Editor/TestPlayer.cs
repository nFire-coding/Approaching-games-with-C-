using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class TestPlayer
{
    private PlayerController _player;
    [SetUp]
    public void PlayerSetUp()
    {
        _player = new PlayerController();
        _player.GroundLayer = "Ground";
        _player.MaxSpeed = 2;
        _player.CoinBoost = 0.1f;
        _player.MaxHealth = 3;
        _player.FireRate = 0.3f;
        _player.JumpHeight = 1f;
        _player.GunTip = new Vector3(0.2f, 0, 0);
    }
    [Test]
    public void TestNull()
    {
        Assert.That(_player != null);
    }

    [Test]
    public void TestGetSet()
    {
        Assert.That(_player.GroundLayer == "Ground");
        Assert.That(_player.MaxSpeed == 2);
        Assert.That(_player.CoinBoost == 0.1f);
        Assert.That(_player.MaxHealth == 3);
        Assert.That(_player.FireRate == 0.3f);
        Assert.That(_player.JumpHeight == 1f);
        Assert.That(_player.GunTip == new Vector3(0.2f, 0, 0));
    }
}
